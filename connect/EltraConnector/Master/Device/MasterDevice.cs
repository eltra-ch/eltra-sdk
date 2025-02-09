﻿using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.ToolSet;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.DeviceDescription;
using EltraConnector.Master.Device.Commands;
using EltraConnector.Master.Device.ParameterConnection;
using EltraConnector.Master.Device.SerialNumber;
using EltraConnector.SyncAgent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace EltraConnector.Master.Device
{
    public class MasterDevice : EltraDevice
    {
        #region Private fields

        private readonly XddDeviceDescriptionFile _deviceDescriptionFile;
        private SyncCloudAgent _cloudAgent;
        private List<DeviceToolPayload> _deviceToolPayloadList;
        
        #endregion

        #region Constructors

        public MasterDevice(string family, string deviceDescriptionFilePath, int nodeId)
        {
            const string methodName = "MasterDevice";

            NodeId = nodeId;
            Family = family;
            DeviceDescriptionFilePath = deviceDescriptionFilePath;

            var xddFamily = _deviceDescriptionFile.ReadFamily();

            if(family != xddFamily)
            {
                MsgLogger.WriteWarning($"{GetType().Name} - {methodName}", $"family specified = {family} differs from xdd definition = {xddFamily}");
            }

            _deviceDescriptionFile = new XddDeviceDescriptionFile(this) { SourceFile = deviceDescriptionFilePath };

            CreateCommandSet();
        }

        public MasterDevice(ISerialNumberProvider serialNumberProvider, string deviceDescriptionFilePath, int nodeId)
        {
            NodeId = nodeId;

            Identification.SerialNumber = serialNumberProvider.ReadSerialNumber();

            DeviceDescriptionFilePath = deviceDescriptionFilePath;
            _deviceDescriptionFile = new XddDeviceDescriptionFile(this) { SourceFile = deviceDescriptionFilePath };

            Family = _deviceDescriptionFile.ReadFamily();

            CreateCommandSet();
        }

        #endregion

        #region Properties

        public MasterDeviceCommunication Communication { get; set; }

        public ParameterConnectionManager ParameterConnectionManager { get; private set; }

        public SyncCloudAgent CloudAgent
        {
            get => _cloudAgent;
            set
            {
                _cloudAgent = value;

                OnCloudAgentChanged();
            }
        }

        public string DeviceDescriptionFilePath { get; set; }

        public string DeviceToolPayloadsPath { get; set; }

        public List<DeviceToolPayload> DeviceToolPayloadList => _deviceToolPayloadList ?? (_deviceToolPayloadList = new List<DeviceToolPayload>());

        #endregion

        #region Events

        public event EventHandler Initialized;

        #endregion

        #region Events handling

        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, new EventArgs());
        }

        protected virtual void OnCloudAgentChanged()
        {
            if (CloudAgent != null)
            {
                Task.Run(async () => {

                    if(CloudAgent!=null)
                    {
                        ChannelId = CloudAgent.ChannelId;
                    }

                    if(!await ReadDeviceDescriptionFile())
                    {
                        MsgLogger.WriteError($"{GetType().Name} - OnCloudAgentChanged", "Read device description failed!");
                    }
                });
            }
        }

        protected virtual void CreateCommunication()
        {
        }

        #endregion

        #region Methods

        private void CreateCommandSet()
        {
            AddCommand(new RegisterParameterUpdateCommand(this));
            AddCommand(new UnregisterParameterUpdateCommand(this));

            AddCommand(new GetObjectCommand(this));
            AddCommand(new SetObjectCommand(this));
        }

        public override async Task<bool> ReadDeviceDescriptionFile()
        {
            _deviceDescriptionFile.Url = CloudAgent.Url;
            
            StatusChanged += (sender, args) => 
            {
                if(Status == DeviceStatus.Ready)
                {
                    CreateConnectionManager();

                    AddDeviceTools(_deviceDescriptionFile);

                    CreateCommunication();

                    OnInitialized();
                }
                else if(Status == DeviceStatus.Registered)
                {
                    UploadToolset();
                }
            };

            return await ReadDeviceDescriptionFile(_deviceDescriptionFile);
        }

        private void AddDeviceTools(XddDeviceDescriptionFile xdd)
        {
            if (xdd != null)
            {
                foreach (var deviceTool in xdd.DeviceTools)
                {
                    AddTool(deviceTool);
                }
            }
        }

        private void CreateConnectionManager()
        {
            ParameterConnectionManager = new ParameterConnectionManager(this);
        }
               
        protected void StartParameterConnectionManager(ref List<Task> tasks)
        {
            var task = ParameterConnectionManager.StartAsync();

            if (task != null)
            {
                tasks.Add(task);
            }
        }

        public bool ReadParameter(Parameter parameter)
        {
            bool result = false;

            if (ParameterConnectionManager != null)
            {
                result = ParameterConnectionManager.ReadParameter(parameter);
            }

            return result;
        }

        public bool WriteParameter(Parameter parameter)
        {
            bool result = false;

            if (ParameterConnectionManager != null)
            {
                result = ParameterConnectionManager.WriteParameter(parameter);
            }

            return result;
        }

        public bool ReadParameterValue<T>(Parameter parameter, out T value)
        {
            bool result = false;

            value = default;

            if (ParameterConnectionManager != null && ParameterConnectionManager.ReadParameter(parameter))
            {
                result = parameter.GetValue(out value);
            }

            return result;
        }

        public bool WriteParameterValue<T>(Parameter parameter, T value)
        {
            bool result = false;

            if (ParameterConnectionManager != null && parameter.SetValue(value))
            {
                result = ParameterConnectionManager.WriteParameter(parameter);
            }

            return result;
        }

        protected virtual void StartConnectionManagersAsync(ref List<Task> tasks)
        {
        }
            
        public override async void RunAsync()
        {
            const string methodName = "RunAsync";

            var tasks = new List<Task>();

            StartParameterConnectionManager(ref tasks);

            if(!UpdateDeviceParameters())
            {
                MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"update device parameters failed!");
            }

            StartConnectionManagersAsync(ref tasks);

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// UpdateDeviceParameters
        /// </summary>
        /// <returns></returns>
        private bool UpdateDeviceParameters()
        {
            const string methodName = "UpdateDeviceParameters";
            bool result = true;
            var parameters = ObjectDictionary?.Parameters;

            if (parameters != null && Communication != null)
            {
                foreach (var parameter in parameters)
                {
                    if (parameter is Parameter parameterEntry)
                    {
                        if (parameterEntry.GetValue(out byte[] bytes) && bytes != null && bytes.Length > 0 && 
                            Communication.GetObject(parameter.Index, parameterEntry.SubIndex, ref bytes))
                        {
                            if(!parameterEntry.SetValue(bytes))
                            {
                                MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"set value failed! 0x{parameterEntry.Index:X4}:0x{parameterEntry.SubIndex:X2}");
                                result = false;
                            }
                        }
                    }
                    else if (parameter is StructuredParameter structuredParameter)
                    {
                        var subParameters = structuredParameter.Parameters;
                        if (subParameters != null)
                        {
                            foreach (var subParameter in subParameters)
                            {
                                if (subParameter is Parameter subParameterEntry &&
                                    subParameterEntry.SubIndex > 0 &&
                                    subParameterEntry.GetValue(out byte[] bytes) && bytes != null && bytes.Length > 0 &&
                                    Communication.GetObject(subParameterEntry.Index, subParameterEntry.SubIndex, ref bytes))
                                {
                                    if(!subParameterEntry.SetValue(bytes))
                                    {
                                        MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"set value failed! 0x{subParameterEntry.Index:X4}:0x{subParameterEntry.SubIndex:X2}");
                                        result = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                result = false;
                MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"no object dictionary or communication set!");
            }

            return result;
        }

        public virtual void Disconnect()
        {
            if (ParameterConnectionManager.IsRunning)
            {
                ParameterConnectionManager.Stop();
            }

            Status = DeviceStatus.Disconnected;
        }

        public virtual int GetUpdateInterval(ParameterUpdatePriority priority)
        {
            int result;

            switch (priority)
            {
                case ParameterUpdatePriority.High:
                    result = 10000;
                    break;
                case ParameterUpdatePriority.Medium:
                    result = 30000;
                    break;
                case ParameterUpdatePriority.Low:
                    result = 60000;
                    break;
                case ParameterUpdatePriority.Lowest:
                    result = 180000;
                    break;
                default:
                    result = 60000;
                    break;
            }

            return result;
        }

        protected bool AddLocalPayload(string fullPath, string id, DeviceToolPayloadMode mode)
        {
            bool result = false;

            try
            {
                if (File.Exists(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(fullPath);
                    var payload = new DeviceToolPayload();

                    var bytes = File.ReadAllBytes(fullPath);

                    payload.Id = id;
                    payload.Mode = mode;
                    payload.FileName = fileInfo.Name;
                    payload.Version = fileVersionInfo.FileVersion;
                    payload.Content = Convert.ToBase64String(bytes);
                    payload.HashCode = CryptHelpers.ToMD5(payload.Content);

                    MsgLogger.WriteFlow($"{GetType().Name} - AddLocalPayload", $"payload added, file name = {payload.FileName}, hashCode = {payload.HashCode}, version = {payload.Version}");

                    DeviceToolPayloadList.Add(payload);

                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddLocalPayload", e);
            }

            return result;
        }

        protected bool UpdatePayloadFromFile(string fullPath, DeviceToolPayload payload)
        {
            bool result = false;

            try
            {
                if (File.Exists(fullPath))
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(fullPath);

                    var bytes = File.ReadAllBytes(fullPath);

                    payload.Version = fileVersionInfo.FileVersion;
                    payload.Content = Convert.ToBase64String(bytes);
                    payload.HashCode = CryptHelpers.ToMD5(payload.Content);

                    MsgLogger.WriteFlow($"{GetType().Name} - UpdatePayloadFromFile", $"payload added, file name = {payload.FileName}, hashCode = {payload.HashCode}, version = {payload.Version}");

                    DeviceToolPayloadList.Add(payload);

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - UpdatePayloadFromFile", $"path = '{fullPath}' doesn't exist! cannot add payload!");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UpdatePayloadFromFile", e);
            }

            return result;
        }

        private DeviceToolPayload FindLocalPayload(DeviceToolPayload payload)
        {
            DeviceToolPayload result = null;

            foreach (var deviceToolPayload in DeviceToolPayloadList)
            {
                if (deviceToolPayload.Mode == DeviceToolPayloadMode.Development)
                {
                    if (deviceToolPayload.Id == payload.Id && deviceToolPayload.Content.Length > 0)
                    {
                        result = deviceToolPayload;
                        break;
                    }
                }
                else
                {
                    if (deviceToolPayload.Id == payload.Id &&
                        deviceToolPayload.HashCode == payload.HashCode &&
                        deviceToolPayload.Content.Length > 0)
                    {
                        result = deviceToolPayload;
                        break;
                    }
                }
            }

            return result;
        }

        protected virtual bool UpdatePayloadContent(DeviceToolPayload payload)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(DeviceToolPayloadsPath) && Directory.Exists(DeviceToolPayloadsPath))
            {
                string path = Path.Combine(DeviceToolPayloadsPath, payload.FileName);

                result = UpdatePayloadFromFile(path, payload);

                if(!result)
                {
                    MsgLogger.WriteError($"{GetType().Name} - UpdatePayloadContent", $"update file '{path}' failed!");
                }
            }
            else
            {
                MsgLogger.WriteWarning($"{GetType().Name} - UpdatePayloadContent", $"payloads path not found '{DeviceToolPayloadsPath}'");
            }

            if (!result)
            {
                var deviceToolPayload = FindLocalPayload(payload);

                if (deviceToolPayload != null)
                {
                    payload.Content = deviceToolPayload.Content;
                    payload.Version = deviceToolPayload.Version;

                    result = true;
                }
                else
                {
                    MsgLogger.WriteWarning($"{GetType().Name} - UpdatePayloadContent", $"local payload not found, file name = {payload.FileName}, hashCode = {payload.HashCode}, version = {payload.Version}");
                }
            }

            return result;
        }

        private void UploadToolset()
        {
            Task.Run(async () => {

                var agent = CloudAgent;

                foreach (var tool in ToolSet.Tools)
                {
                    if (tool.Status == DeviceToolStatus.Enabled)
                    {
                        foreach (var payload in tool.PayloadSet)
                        {
                            if (UpdatePayloadContent(payload))
                            {
                                payload.ToolId = tool.Id;
                                payload.ChannelId = agent.ChannelId;
                                payload.NodeId = NodeId;

                                if (!await agent.PayloadExists(payload))
                                {
                                    if (!await agent.UploadPayload(payload))
                                    {
                                        MsgLogger.WriteError($"{GetType().Name} - UploadToolset", $"UploadPayload {ChannelId}:{NodeId}, file name = {payload.FileName} failed!");
                                    }
                                    else
                                    {
                                        MsgLogger.WriteFlow($"{GetType().Name} - UploadToolset", $"payload name = {payload.FileName}, version = {payload.Version}, successfully uploaded");
                                    }
                                }
                                else
                                {
                                    MsgLogger.WriteFlow($"{GetType().Name} - UploadToolset", $"payload name = {payload.FileName}, version = {payload.Version}, already uploaded");
                                }
                            }
                            else
                            {
                                MsgLogger.WriteError($"{GetType().Name} - UploadToolset", $"payload update failed, {payload.FileName}");
                            }
                        }
                    }
                }

            });

        }

        #endregion
    }
}

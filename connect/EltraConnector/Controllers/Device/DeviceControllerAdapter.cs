using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using EltraConnector.Events;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Contracts.Devices;
using EltraCommon.Helpers;
using EltraCommon.Extensions;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.DeviceDescription;
using EltraCommon.ObjectDictionary.DeviceDescription.Factory;
using EltraCommon.Contracts.History;
using System.Threading;
using EltraCommon.Contracts.Users;
using EltraCommon.Contracts.ToolSet;
using EltraCommon.Transport;
using EltraConnector.Transport.Ws.Interfaces;
using System.Net;
using EltraCommon.Contracts.Parameters;

namespace EltraConnector.Controllers.Device
{
    internal class DeviceControllerAdapter : CloudChannelControllerAdapter
    {
        #region Private fields

        private readonly UserIdentity _identity;
        private readonly IHttpClient _httpClient;

        private EltraDeviceSet _channelDevices;
        private DeviceCommandsControllerAdapter _deviceCommandsControllerAdapter;
        private ParameterControllerAdapter _parameterControllerAdapter;
        private DescriptionControllerAdapter _descriptionContollerAdapter;
        private IConnectionManager _connectionManager;
        
        #endregion

        #region Constructors

        public DeviceControllerAdapter(IHttpClient httpClient, string url, Channel channel, UserIdentity identity)
            : base(httpClient, url, channel)
        {
            _httpClient = httpClient;
            _identity = identity;
        }

        #endregion

        #region Properties

        public IConnectionManager ConnectionManager 
        { 
            get => _connectionManager;
            set 
            {
                _connectionManager = value;
                OnConnectionManagerChanged();
            }
        }

        private EltraDeviceSet ChannelDevices => _channelDevices ?? (_channelDevices = new EltraDeviceSet { Channel = Channel });

        public DeviceCommandsControllerAdapter DeviceCommandsAdapter => _deviceCommandsControllerAdapter ?? (_deviceCommandsControllerAdapter = CreateDeviceCommandsAdapter());

        public ParameterControllerAdapter ParameterAdapter => _parameterControllerAdapter ?? (_parameterControllerAdapter = CreateParameterAdapter());

        public DescriptionControllerAdapter DescriptionContollerAdapter => _descriptionContollerAdapter ?? (_descriptionContollerAdapter = CreateDescriptionAdapter());

        protected UserIdentity Identity => _identity;

        #endregion

        #region Events

        public event EventHandler<RegistrationEventArgs> RegistrationStateChanged;

        #endregion

        #region Events handling

        private void OnConnectionManagerChanged()
        {
            DeviceCommandsAdapter.ConnectionManager = ConnectionManager;
        }

        

        protected virtual void OnRegistrationStateChanged(RegistrationEventArgs e)
        {
            switch(e.State)
            {
                case RegistrationState.Registered:
                    MsgLogger.WriteFlow($"{GetType().Name} - OnRegistrationStateChanged", $"device '{e.Device.Name}' registered successfully.");
                    break;
                case RegistrationState.Unregistered:
                    MsgLogger.WriteFlow($"{GetType().Name} - OnRegistrationStateChanged", $"device '{e.Device.Name}' unregistered successfully.");
                    break;
                case RegistrationState.Failed:
                    if(e.Exception!=null)
                    {
                        MsgLogger.Exception($"{GetType().Name} - OnRegistrationStateChanged", e.Exception);
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - OnRegistrationStateChanged", $"device '{e.Device.Name}' register failed, reason = {e.Reason}!");
                    }
                    break;
            }

            RegistrationStateChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        protected virtual DeviceCommandsControllerAdapter CreateDeviceCommandsAdapter()
        {
           return null;
        }

        private ParameterControllerAdapter CreateParameterAdapter()
        {
            var adapter = new ParameterControllerAdapter(_httpClient, _identity, Url, Channel);

            AddChild(adapter);

            return adapter;
        }

        private DescriptionControllerAdapter CreateDescriptionAdapter()
        {
            var adapter = new DescriptionControllerAdapter(_httpClient, _identity, Url);

            AddChild(adapter);

            return adapter;
        }

        public override bool Stop()
        {
            _deviceCommandsControllerAdapter?.Stop();
            _parameterControllerAdapter?.Stop();

            return base.Stop();
        }

        public async Task<bool> UnregisterChannelDevice(EltraDevice deviceNode)
        {
            var device = deviceNode;
            var query = HttpUtility.ParseQueryString(string.Empty);

            var url = UrlHelper.BuildUrl(Url, $"api/device/unregister/{Channel.Id}/{device.NodeId}", query);

            bool result = await Transporter.Delete(_identity, url, CancellationToken.None);

            if (result)
            {
                ChannelDevices.RemoveDevice(deviceNode);
            }

            return result;
        }

        public async Task<List<EltraDevice>> GetDeviceNodes(string channelId)
        {
            var result = new List<EltraDevice>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["channelId"] = channelId;

                var url = UrlHelper.BuildUrl(Url, "api/channel/devices", query);
                var json = await Transporter.Get(_identity, url);

                var devices = json.TryDeserializeObject<EltraDeviceList>();

                if (devices != null)
                {
                    foreach (var device in devices.Items)
                    {                        
                        var deviceDescriptionFile = DeviceDescriptionFactory.CreateDeviceDescriptionFile(device);

                        if (deviceDescriptionFile != null)
                        {
                            deviceDescriptionFile.Url = Url;

                            if(!await device.ReadDeviceDescriptionFile(deviceDescriptionFile))
                            {
                                MsgLogger.WriteError($"{GetType().Name} - GetChannelDevices", "read device description file failed!");
                            }
                        }
                        else
                        {
                            device.Status = DeviceStatus.Ready;
                        }

                        result.Add(device);
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetChannelDevices", e);
            }

            return result;
        }

        internal Task<DeviceDescriptionPayload> DownloadDeviceDescription(string channelId, DeviceVersion deviceVersion)
        {
            return DescriptionContollerAdapter.Download(channelId, deviceVersion);
        }

        internal Task<DeviceDescriptionIdentity> GetDeviceDescriptionIdentity(string channelId, DeviceVersion deviceVersion)
        {
            return DescriptionContollerAdapter.GetIdentity(channelId, deviceVersion);
        }

        public async Task<bool> RegisterDevice(EltraDevice deviceNode)
        {
            bool result = false;

            try
            {
                var device = deviceNode;

                if (await UploadDeviceDescription(device))
                {
                    deviceNode.ChannelId = Channel.Id;

                    var path = "api/device/register";
                    var postResult = await Transporter.Post(_identity, Url, path, deviceNode.ToJson());

                    if (postResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        result = true;
                    }

                    if (result)
                    {
                        ChannelDevices.AddDevice(device);

                        OnRegistrationStateChanged(new RegistrationEventArgs { Channel = Channel, Device = device, State = RegistrationState.Registered });
                    }
                    else
                    {
                        OnRegistrationStateChanged(new RegistrationEventArgs
                        {
                            Channel = Channel,
                            Device = device,
                            Exception = postResult.Exception,
                            Reason = $"post failed, status code = {postResult.StatusCode}",
                            State = RegistrationState.Failed
                        });
                    }
                }
                else
                {
                    OnRegistrationStateChanged(new RegistrationEventArgs
                    {
                        Channel = Channel,
                        Device = device,                        
                        State = RegistrationState.Failed,
                        Reason = "upload device description failed",
                    });
                }
            }
            catch (Exception e)
            {
                OnRegistrationStateChanged(new RegistrationEventArgs { Channel = Channel, 
                                                                       Device = deviceNode, 
                                                                       Exception = e, 
                                                                       Reason = "exception",
                                                                       State = RegistrationState.Failed });
            }

            return result;
        }

        public async Task<bool> UpdateDevice(EltraDevice deviceNode)
        {
            bool result = false;

            try
            {
                var device = deviceNode;

                if (await UploadDeviceDescription(device))
                {
                    deviceNode.ChannelId = Channel.Id;

                    ChannelDevices.AddDevice(device);

                    result = true;

                    OnRegistrationStateChanged(new RegistrationEventArgs { Channel = Channel, Device = device, State = RegistrationState.Registered });
                }
                else
                {
                    OnRegistrationStateChanged(new RegistrationEventArgs
                    {
                        Channel = Channel,
                        Device = device,
                        State = RegistrationState.Failed,
                        Reason = "upload device description failed",
                    });
                }
            }
            catch (Exception e)
            {
                OnRegistrationStateChanged(new RegistrationEventArgs
                {
                    Channel = Channel,
                    Device = deviceNode,
                    Exception = e,
                    Reason = "exception",
                    State = RegistrationState.Failed
                });
            }

            return result;
        }

        private async Task<bool> UploadDeviceDescription(EltraDevice device)
        {
            bool result = false;
            var deviceDescriptionPayload = new DeviceDescriptionPayload(device)
            {
                ChannelId = Channel.Id
            };

            if (string.IsNullOrEmpty(deviceDescriptionPayload.Content))
            {
                MsgLogger.WriteError($"{GetType().Name} - UploadDeviceDescription", "Missing device description content!");
            }
            else
            {
                if (!await DescriptionContollerAdapter.Exists(deviceDescriptionPayload))
                {
                    result = await DescriptionContollerAdapter.Upload(deviceDescriptionPayload);

                    if (!result)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - UploadDeviceDescription", "Upload device description failed!");
                    }
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - UploadDeviceDescription", "Device description already exists");
                    result = true;
                }
            }

            return result;
        }

        public async Task<DeviceStatus> GetDeviceStatus(EltraDevice device)
        {
            var result = DeviceStatus.Undefined;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                var deviceVersion = device.Version;
                var identification = device.Identification;

                query["channelId"] = Channel.Id;
                query["nodeId"] = $"{device.NodeId}";
                query["deviceName"] = $"{device.Name}";
                query["hardwareVersion"] = $"{deviceVersion.HardwareVersion}";
                query["softwareVersion"] = $"{deviceVersion.SoftwareVersion}";
                query["applicationNumber"] = $"{deviceVersion.ApplicationNumber}";
                query["applicationVersion"] = $"{deviceVersion.ApplicationVersion}";
                query["serialNumber"] = $"{identification.SerialNumber}";

                var url = UrlHelper.BuildUrl(Url, "api/device/status", query);

                var json = await Transporter.Get(_identity, url);

                if(!string.IsNullOrEmpty(json))
                {
                    var statusUpdate = json.TryDeserializeObject<DeviceStatusUpdate>();

                    if(statusUpdate!=null)
                    {
                        result = statusUpdate.Status;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetDeviceStatus", e);
            }

            return result;
        }
        
        public async Task<bool> RegisterDevices()
        {
            bool result = true;

            try
            {
                foreach (var channelDevice in ChannelDevices.Devices)
                {
                    var status = await GetDeviceStatus(channelDevice);

                    channelDevice.ChannelId = Channel.Id;

                    if (status != DeviceStatus.Ready && status != DeviceStatus.Registered)
                    {
                        result = await RegisterDevice(channelDevice);
                    }

                    if (!result)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - RegisterDevices", e);
            }
            
            return result;
        }

        public async Task<Parameter> GetParameter(string channelId, int nodeId, ushort index, byte subIndex)
        {
            return await ParameterAdapter.GetParameter(channelId, nodeId, index, subIndex);
        }

        public async Task<ParameterValue> GetParameterValue(string channelId, int nodeId, ushort index, byte subIndex)
        {
            return await ParameterAdapter.GetParameterValue(channelId, nodeId, index, subIndex);
        }

        public async Task<List<ParameterValue>> GetParameterHistory(string channelId, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            return await ParameterAdapter.GetParameterHistory(channelId, nodeId, uniqueId, from, to);
        }
        public async Task<ParameterValueHistoryStatistics> GetParameterHistoryStatistics(string channelId, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            return await ParameterAdapter.GetParameterHistoryStatistics(channelId, nodeId, uniqueId, from, to);
        }

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDevice device)
        {
            return await DeviceCommandsAdapter.GetDeviceCommands(device);
        }

        public async Task<DeviceCommand> GetDeviceCommand(EltraDevice device, string commandName)
        {
            return await DeviceCommandsAdapter.GetDeviceCommand(device, commandName);
        }

        public async Task<bool> PushCommand(ExecuteCommand execCommand, ExecCommandStatus status)
        {
            bool result = false;

            try
            {
                var command = execCommand.Command;

                if (command != null)
                {
                    command.Status = status;

                    if (string.IsNullOrEmpty(execCommand.TargetChannelId))
                    {
                        execCommand.TargetChannelId = command.Device.ChannelId;
                    }
                }

                execCommand.SourceChannelId = Channel.Id;

                result = await DeviceCommandsAdapter.PushCommand(execCommand);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - PushCommand", e);
            }
            
            return result;
        }

        public async Task<bool> PushCommand(ExecuteCommand execCommand)
        {
            execCommand.SourceChannelId = Channel.Id;

            return await DeviceCommandsAdapter.PushCommand(execCommand);
        }

        public async Task<bool> SetCommandStatus(ExecuteCommandStatus status)
        {
            status.ChannelId = Channel.Id;

            return await DeviceCommandsAdapter.SetCommandStatus(status);
        }

        public async Task<bool> SetCommandStatus(ExecuteCommand command, ExecCommandStatus status)
        {
            bool result = false;

            try
            {
                var commandStatus = new ExecuteCommandStatus(Channel.Id, command) { Status = status };

                result = await DeviceCommandsAdapter.SetCommandStatus(commandStatus);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SetCommandStatus", e);
            }
            
            return result;
        }

        public async Task<List<ExecuteCommand>> PopCommands(EltraDevice device, ExecCommandStatus status)
        {
            var result = new List<ExecuteCommand>();
            
            try
            {
                result = await DeviceCommandsAdapter.PullCommands(device, status);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - PopCommands", e);
            }
            
            return result;
        }

        internal Task<bool> UploadPayload(DeviceToolPayload payload)
        {
            return DescriptionContollerAdapter.UploadPayload(payload);
        }

        internal Task<bool> PayloadExists(DeviceToolPayload payload)
        {
            return DescriptionContollerAdapter.PayloadExists(payload);
        }

        public async Task<bool> SetParameterValue(int nodeId, ushort index, byte subIndex, ParameterValue actualValue)
        {
            bool result = false;

            try
            {
                var parameterUpdate = new ParameterValueUpdate
                {
                    ChannelId = Channel.Id,
                    NodeId = nodeId,
                    ParameterValue = actualValue,
                    Index = index,
                    SubIndex = subIndex
                };

                var path = $"api/parameter/value";

                var json = parameterUpdate.ToJson();

                var response = await Transporter.Put(Identity, Url, path, json);

                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - UpdateParameterValue", $"Device Node id = {nodeId}, Parameter Index = 0x{index:X4}, Subindex = 0x{subIndex} value successfully written");

                        result = true;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - UpdateParameterValue", $"Device Node id = {nodeId}, Parameter Index = 0x{index:X4}, Subindex = 0x{subIndex} value write failed, status code = {response.StatusCode}!");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - UpdateParameterValue", $"Device Node id = {nodeId}, Parameter Index = 0x{index:X4}, Subindex = 0x{subIndex} value write failed!");
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        #endregion        
    }
}
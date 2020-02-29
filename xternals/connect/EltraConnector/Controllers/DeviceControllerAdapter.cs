﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using EltraConnector.Controllers.Base;
using EltraConnector.Events;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Sessions;
using EltraCommon.Logger;
using Newtonsoft.Json;
using EltraCloudContracts.Contracts.Devices;
using EltraCommon.Helpers;
using EltraConnector.Extensions;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Factory;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.Contracts.Users;

namespace EltraConnector.Controllers
{
    public class DeviceControllerAdapter : CloudSessionControllerAdapter
    {
        #region Private fields

        private SessionDevices _sessionDevices;
        private DeviceCommandsControllerAdapter _deviceCommandsControllerAdapter;
        private ParameterControllerAdapter _parameterControllerAdapter;
        private DescriptionControllerAdapter _descriptionContollerAdapter;

        #endregion

        #region Constructors

        public DeviceControllerAdapter(string url, Session session)
            : base(url, session)
        {
        }

        #endregion

        #region Properties

        private SessionDevices SessionDevices => _sessionDevices ?? (_sessionDevices = new SessionDevices { SessionUuid = Session.Uuid });

        public DeviceCommandsControllerAdapter DeviceCommandsAdapter => _deviceCommandsControllerAdapter ?? (_deviceCommandsControllerAdapter = CreateDeviceCommandsAdapter());

        public ParameterControllerAdapter ParameterAdapter => _parameterControllerAdapter ?? (_parameterControllerAdapter = CreateParameterAdapter());

        public DescriptionControllerAdapter DescriptionContollerAdapter => _descriptionContollerAdapter ?? (_descriptionContollerAdapter = CreateDescriptionAdapter());

        #endregion

        #region Events

        public event EventHandler<RegistrationEventArgs> RegistrationStateChanged;

        #endregion

        #region Events handling

        protected virtual void OnRegistrationStateChanged(RegistrationEventArgs e)
        {
            switch(e.State)
            {
                case RegistrationState.Registered:
                    MsgLogger.WriteFlow($"device '{e.Device.Name}' registered successfully.");
                    break;
                case RegistrationState.Unregistered:
                    MsgLogger.WriteFlow($"device '{e.Device.Name}' unregistered successfully.");
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

        private DeviceCommandsControllerAdapter CreateDeviceCommandsAdapter()
        {
            var adapter = new DeviceCommandsControllerAdapter(Url, Session);

           AddChild(adapter);

           return adapter;
        }

        private ParameterControllerAdapter CreateParameterAdapter()
        {
            var adapter = new ParameterControllerAdapter(Url, Session);

            AddChild(adapter);

            return adapter;
        }

        private DescriptionControllerAdapter CreateDescriptionAdapter()
        {
            var adapter = new DescriptionControllerAdapter(Url);

            AddChild(adapter);

            return adapter;
        }

        public override bool Stop()
        {
            _deviceCommandsControllerAdapter?.Stop();
            _parameterControllerAdapter?.Stop();

            return base.Stop();
        }

        public async Task UnregisterSessionDevice(EltraDevice device)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            var url = UrlHelper.BuildUrl(Url, $"api/device/remove/{Session.Uuid}/{device.Identification.SerialNumber}", query);

            await Transporter.Delete(url);

            SessionDevices.RemoveDevice(device);
        }

        public async Task<List<EltraDevice>> GetSessionDevices(string uuid, UserAuthData authData)
        {
            var devices = new List<EltraDevice>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uuid"] = uuid;
                query["login"] = authData.Login;
                query["password"] = authData.Password;

                var url = UrlHelper.BuildUrl(Url, "api/session/devices", query);
                var json = await Transporter.Get(url);

                devices = JsonConvert.DeserializeObject<List<EltraDevice>>(json);

                if (devices != null)
                {
                    foreach (var device in devices)
                    {
                        var deviceDescriptionFile = DeviceDescriptionFactory.CreateDeviceDescriptionFile(device);

                        if (deviceDescriptionFile != null)
                        {
                            deviceDescriptionFile.Url = Url;

                            if(!await device.ReadDeviceDescriptionFile(deviceDescriptionFile))
                            {
                                MsgLogger.WriteError($"{GetType().Name} - GetSessionDevices", "read device description file failed!");
                            }
                        }
                        else
                        {
                            device.Status = DeviceStatus.Ready;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetSessionDevices", e);
            }

            return devices;
        }
        
        public async Task<bool> RegisterDevice(EltraDevice device)
        {
            bool result = false;

            try
            {
                if (await UploadDeviceDescription(device))
                {
                    var sessionDevice = new SessionDevice { SessionUuid = Session.Uuid, Device = device };
                    var path = "api/device/add";
                    var postResult = await Transporter.Post(Url, path, sessionDevice.ToJson());

                    if (postResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        result = bool.TryParse(postResult.Content, out result);
                    }

                    if (result)
                    {
                        SessionDevices.AddDevice(device);

                        OnRegistrationStateChanged(new RegistrationEventArgs { Session = Session, Device = device, State = RegistrationState.Registered });
                    }
                    else
                    {
                        OnRegistrationStateChanged(new RegistrationEventArgs
                        {
                            Session = Session,
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
                        Session = Session,
                        Device = device,                        
                        State = RegistrationState.Failed,
                        Reason = "upload device description failed",
                    });
                }
            }
            catch (Exception e)
            {
                OnRegistrationStateChanged(new RegistrationEventArgs { Session = Session, 
                                                                       Device = device, 
                                                                       Exception = e, 
                                                                       Reason = "exception",
                                                                       State = RegistrationState.Failed });
            }

            return result;
        }

        private async Task<bool> UploadDeviceDescription(EltraDevice device)
        {
            bool result;
            var deviceDescriptionPayload = new DeviceDescriptionPayload(device);

            if (!await DescriptionContollerAdapter.Exists(deviceDescriptionPayload))
            {
                deviceDescriptionPayload.CallerUuid = Session.Uuid;

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

            return result;
        }

        public async Task<bool> IsDeviceRegistered(string uuid, EltraDevice device)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["Uuid"] = uuid;
                query["SessionUuid"] = Session.Uuid;
                query["SerialNumber"] = $"{device.Identification.SerialNumber}";

                var url = UrlHelper.BuildUrl(Url, "api/device/exists", query);

                var json = await Transporter.Get(url);

                bool.TryParse(json, out result);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - IsDeviceRegistered", e);
            }

            return result;
        }
        
        public async Task<bool> RegisterDevices()
        {
            bool result = true;

            try
            {
                foreach (var device in SessionDevices.Devices)
                {
                    result = await RegisterDevice(device);

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

        public async Task<Parameter> GetParameter(ulong serialNumber, ushort index, byte subIndex)
        {
            return await ParameterAdapter.GetParameter(serialNumber, index, subIndex);
        }

        public async Task<ParameterValue> GetParameterValue(ulong serialNumber, ushort index, byte subIndex)
        {
            return await ParameterAdapter.GetParameterValue(serialNumber, index, subIndex);
        }

        public async Task<List<ParameterUniqueIdValuePair>> GetParameterHistoryPair(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            return await ParameterAdapter.GetParameterHistoryPair(serialNumber, uniqueId1, uniqueId2, from, to);
        }

        public async Task<List<ParameterValue>> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to)
        {
            return await ParameterAdapter.GetParameterHistory(serialNumber, uniqueId, from, to);
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
                }
                
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
            return await DeviceCommandsAdapter.PushCommand(execCommand);
        }

        public async Task<bool> SetCommandStatus(ExecuteCommandStatus status)
        {
            return await DeviceCommandsAdapter.SetCommandStatus(status);
        }

        public async Task<bool> SetCommandStatus(ExecuteCommand command, ExecCommandStatus status)
        {
            bool result = false;

            try
            {
                var commandStatus = new ExecuteCommandStatus(command) { Status = status };

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
                result = await DeviceCommandsAdapter.PopCommands(device, status);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - PopCommands", e);
            }
            
            return result;
        }

        public async Task<bool> LockDevice(string agentUuid, EltraDevice eposDevice)
        {
            bool result = false;

            try
            {
                var deviceLock = new DeviceLock { AgentUuid = agentUuid, SerialNumber = eposDevice.Identification.SerialNumber };

                var path = "api/device/lock";
                var postResult = await Transporter.Post(Url, path, JsonConvert.SerializeObject(deviceLock));

                if (postResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(postResult.Content);

                    result = requestResult.Result;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - LockDevice", e);
            }

            return result;
        }

        public async Task<bool> UnlockDevice(string agentUuid, EltraDevice eposDevice)
        {
            bool result = false;

            try
            {
                var deviceLock = new DeviceLock { AgentUuid = agentUuid, SerialNumber = eposDevice.Identification.SerialNumber };

                var path = "api/device/unlock";
                var postResult = await Transporter.Post(Url, path, JsonConvert.SerializeObject(deviceLock));

                if (postResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(postResult.Content);

                    result = requestResult.Result;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UnlockDevice", e);
            }

            return result;
        }

        public async Task<bool> CanLockDevice(string agentUuid, EltraDevice eposDevice)
        {
            bool result = false;

            try
            {
                var deviceLock = new DeviceLock { AgentUuid = agentUuid, SerialNumber = eposDevice.Identification.SerialNumber };

                var path = "api/device/can-agent-lock";
                var postResult = await Transporter.Post(Url, path, JsonConvert.SerializeObject(deviceLock));

                if (postResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(postResult.Content);

                    result = requestResult.Result;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CanLockDevice", e);
            }

            return result;
        }

        public async Task<bool> IsDeviceLocked(string agentUuid, EltraDevice eposDevice)
        {
            bool result = false;

            try
            {
                var deviceLock = new DeviceLock { AgentUuid = agentUuid, SerialNumber = eposDevice.Identification.SerialNumber };

                var path = "api/device/is-locked-by-agent";
                var postResult = await Transporter.Post(Url, path, JsonConvert.SerializeObject(deviceLock));

                if (postResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(postResult.Content);

                    result = requestResult.Result;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - IsDeviceLocked", e);
            }

            return result;
        }

        #endregion

        
    }
}
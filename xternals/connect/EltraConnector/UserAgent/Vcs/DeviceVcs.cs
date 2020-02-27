using System;
using System.Threading;
using System.Threading.Tasks;

using EltraConnector.Classes;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.Contracts.Users;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Events;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Factory;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using System.Collections.Generic;
using EltraCommon.Logger;
using System.Diagnostics;

namespace EltraConnector.UserAgent.Vcs
{
    public class DeviceVcs : IDisposable 
    {
        #region Private fields

        private static object _registeredParameterLocker = new object();

        private const int DefaultTimeout = 30000;
        private EltraDevice _device;
        private List<RegisteredParameter> _registeredParameters;
        private bool _isDeviceLocked;
        private Stopwatch _deviceLockWatch = new Stopwatch();

        #endregion

        #region Constructors

        public DeviceVcs(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
        {
            Timeout = DefaultTimeout;

            Agent = new DeviceAgent(url, uuid, authData, updateInterval, timeout);

            Agent.ParameterChanged += OnParameterChanged;
        }

        public DeviceVcs(DeviceAgent agent, EltraDevice device)
        {
            Timeout = DefaultTimeout;

            _device = device;

            Agent = agent;

            Agent.ParameterChanged += OnParameterChanged;
        }

        #endregion

        #region Properties

        public EltraDevice Device
        {
            get => _device;
            set
            {
                _device = value;
                OnDeviceChanged();
            }
        }

        public DeviceAgent Agent { get; }

        public int Timeout { get; set; }

        private List<RegisteredParameter> RegisteredParameters => _registeredParameters ?? (_registeredParameters = new List<RegisteredParameter>());

        #endregion

        #region Events

        public event EventHandler DeviceChanged;
        
        #endregion

        #region Events handling

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            var objectDictionary = Device?.ObjectDictionary;

            if(objectDictionary!=null && e.Parameter!=null)
            {
                var parameterBase = objectDictionary.SearchParameter(e.Parameter.UniqueId);

                if(parameterBase is Parameter parameterEntry)
                {
                    parameterEntry.ActualValue = e.Parameter.ActualValue;
                }
            }
        }

        protected virtual void OnDeviceChanged()
        {
            DeviceChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
        
        #region Methods
        
        private DeviceCommand FindBufferedVcsCommand(string commandName)
        {
            DeviceCommand result = null;

            foreach (var deviceCommand in Agent.DeviceCommands)
            {
                if (deviceCommand.Name == commandName)
                {
                    result = deviceCommand.Clone();
                    break;
                }
            }

            return result;
        }
        
        protected async Task<DeviceCommand> GetVcsCommand(string commandName)
        {
            DeviceCommand result = FindBufferedVcsCommand(commandName);
            
            if (result == null && Device != null)
            {
                result = await Agent.GetDeviceCommand(Device, commandName);

                var device = result?.Device;

                if (device != null && device.ObjectDictionary == null)
                {
                    await DownloadObjectDictionary(device);
                }
            }

            return result;
        }

        private async Task DownloadObjectDictionary(EltraDevice device)
        {
            var token = new ManualResetEvent(false);

            await Task.Run(async () =>
            {
                var deviceDescription = DeviceDescriptionFactory.CreateDeviceDescriptionFile(device);

                if(deviceDescription!=null)
                { 
                    deviceDescription.StateChanged += (sender, args) =>
                    {
                        if (args.State == DeviceDescriptionState.Read)
                        {
                            if (sender is DeviceDescriptionFile deviceDescriptionFile)
                            {
                                if (device.CreateDeviceDescription(deviceDescriptionFile))
                                {
                                    device.CreateObjectDictionary();
                                }

                                token.Set();
                            }
                        }
                    };

                    await deviceDescription.Read();
                }
                else
                {
                    device.Status = DeviceStatus.Ready;

                    token.Set();
                }
            });

            token.WaitOne(Timeout);
        }

        public async Task<bool> UpdateParameterValue(string uniqueId)
        {
            bool result = false;

            if (Device != null)
            {
                if (Device.SearchParameter(uniqueId) is Parameter parameterEntry)
                {
                    var serialNumber = Device.Identification.SerialNumber;

                    var parameterValue = await Agent.GetParameterValue(serialNumber,
                        parameterEntry.Index, parameterEntry.SubIndex);

                    result = parameterEntry.SetValue(parameterValue);
                }
            }

            return result;
        }

        public async Task<bool> UpdateParameterValue(ushort index, byte subIndex)
        {
            bool result = false;

            if (Device != null)
            {
                if (Device.SearchParameter(index, subIndex) is Parameter parameterEntry)
                {
                    var serialNumber = Device.Identification.SerialNumber;

                    var parameterValue = await Agent.GetParameterValue(serialNumber, index, subIndex);

                    result = parameterEntry.SetValue(parameterValue);
                }
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(string uniqueId)
        {
            ParameterValue result = null;

            if (Device != null)
            {
                if (Device.SearchParameter(uniqueId) is Parameter parameterEntry)
                {
                    result = await Agent.GetParameterValue(Device, parameterEntry.Index, parameterEntry.SubIndex);
                }
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(ushort index, byte subIndex)
        {
            ParameterValue result = null;

            if (Device != null)
            {
                result = await Agent.GetParameterValue(Device, index, subIndex);
            }

            return result;
        }

        public async Task<Parameter> GetParameter(string uniqueId)
        {
            Parameter result = null;

            if (Device != null)
            {
                if (Device.SearchParameter(uniqueId) is Parameter parameterEntry)
                {
                    var parameterValue = await Agent.GetParameterValue(Device, parameterEntry.Index, parameterEntry.SubIndex);

                    if(parameterValue!=null)
                    {
                        if(parameterEntry.SetValue(parameterValue))
                        {
                            result = parameterEntry;
                        }
                    }
                }
            }

            return result;
        }

        public async Task<Parameter> GetParameter(ushort index, byte subIndex)
        {
            Parameter result = null;

            if (Device != null)
            {
                result = await Agent.GetParameter(Device, index, subIndex);
            }

            return result;
        }

        public bool IsParameterRegistered(string uniqueId)
        {
            bool result = false;

            lock (_registeredParameterLocker)
            {
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.UniqueId == uniqueId)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private void AddToRegisteredParameters(string uniqueId)
        {
            lock (_registeredParameterLocker)
            {
                RegisteredParameters.Add(new RegisteredParameter(uniqueId, _registeredParameterLocker));                
            }
        }

        private bool FindRegisteredParameter(string uniqueId, out RegisteredParameter registeredParameter)
        {            
            bool result = false;
                       
            registeredParameter = null;
            
            lock (_registeredParameterLocker)
            { 
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.UniqueId == uniqueId)
                    {
                        registeredParameter = parameter;
                        result = true;
                        break;
                    }                    
                }
            }
        
            return result;
        }

        public void RegisterParameterUpdate(string uniqueId, ParameterUpdatePriority priority = ParameterUpdatePriority.Low)
        {
#pragma warning disable 4014

            Task.Run(async () => {
                bool result = false;

                if (!string.IsNullOrEmpty(uniqueId))
                {   
                    if (!IsParameterRegistered(uniqueId))
                    {
                        if (Device != null)
                        {
                            if (Device.SearchParameter(uniqueId) is Parameter parameterEntry)
                            {
                                var command = await GetVcsCommand("RegisterParameterUpdate");

                                if (command != null)
                                {
                                    command.SetParameterValue("Index", parameterEntry.Index);
                                    command.SetParameterValue("SubIndex", parameterEntry.SubIndex);
                                    command.SetParameterValue("Priority", (int)priority);

                                    result = await Agent.ExecuteCommandAsync(command);
                                }

                                if (result)
                                {
                                    AddToRegisteredParameters(uniqueId);
                                }
                                else
                                {
                                    MsgLogger.WriteError($"{GetType().Name} - RegisterParameterUpdate", $"parameter could't be registered - '{uniqueId}'");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (FindRegisteredParameter(uniqueId, out var registeredParameter))
                        {
                            registeredParameter.InstanceCount++;

                            MsgLogger.WriteDebug($"{GetType().Name} - RegisterParameterUpdate", $"register parameter '{uniqueId}', instance count = {registeredParameter.InstanceCount}");
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - RegisterParameterUpdate", $"register: cannot find registered parameter '{uniqueId}'");
                        }
                    }                
                }

            }).ConfigureAwait(false);

#pragma warning restore 4014
        }

        public void UnregisterParameterUpdate(string uniqueId, ParameterUpdatePriority priority = ParameterUpdatePriority.Low)
        {
            Task.Run(async () =>
            {
               if (!string.IsNullOrEmpty(uniqueId))
               {
                    bool result = false;

                    if (FindRegisteredParameter(uniqueId, out var registeredParameter))
                   {
                       if (registeredParameter.InstanceCount <= 1)
                       {
                           if (Device != null)
                           {
                               if (Device.SearchParameter(uniqueId) is Parameter parameterEntry)
                               {
                                   var command = await GetVcsCommand("UnregisterParameterUpdate");

                                   if (command != null)
                                   {
                                       command.SetParameterValue("Index", parameterEntry.Index);
                                       command.SetParameterValue("SubIndex", parameterEntry.SubIndex);
                                       command.SetParameterValue("Priority", (int)priority);

                                       result = await Agent.ExecuteCommandAsync(command);
                                   }
                               }
                           }

                           if (result)
                           {
                               RemoveFromRegisteredParameters(uniqueId);

                               MsgLogger.WriteDebug($"{GetType().Name} - UnregisterParameterUpdate", $"unregistered parameter '{uniqueId}'");
                           }
                           else
                           {
                               MsgLogger.WriteError($"{GetType().Name} - UnregisterParameterUpdate", $"parameter could't be unregistered - '{uniqueId}'");
                           }
                       }
                       else
                       {
                           if (registeredParameter.InstanceCount > 0)
                           {
                               registeredParameter.InstanceCount--;
                           }
                           else
                           {
                               registeredParameter.InstanceCount = 0;
                           }

                           MsgLogger.WriteDebug($"{GetType().Name} - UnregisterParameterUpdate", $"unregister parameter '{uniqueId}', instance count = {registeredParameter.InstanceCount}");

                           result = true;
                       }
                   }
                   else
                   {
                       MsgLogger.WriteError($"{GetType().Name} - UnregisterParameterUpdate", $"unregister: cannot find registered parameter '{uniqueId}'");
                   }
               }
           }).ConfigureAwait(false);
        }

        private bool RemoveFromRegisteredParameters(string uniqueId)
        {
            bool result = false;

            lock (_registeredParameterLocker)
            {
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.UniqueId == uniqueId)
                    {
                        result = RegisteredParameters.Remove(parameter);
                        break;
                    }
                }
            }

            return result;
        }

        public async Task<bool> RegisterParameterUpdate(ushort index, byte subIndex)
        {
            bool result = false;

            var command = await GetVcsCommand("RegisterParameterUpdate");

            if (command != null)
            {
                command.SetParameterValue("Index", index);
                command.SetParameterValue("SubIndex", subIndex);
                command.SetParameterValue("Priority", ParameterUpdatePriority.Medium);

                result = await Agent.ExecuteCommandAsync(command);
            }

            return result;
        }

        public async Task<bool> UnregisterParameterUpdate(ushort index, byte subIndex)
        {
            bool result = false;

            var command = await GetVcsCommand("UnregisterParameterUpdate");

            if (command != null)
            {
                command.SetParameterValue("Index", index);
                command.SetParameterValue("SubIndex", subIndex);
                command.SetParameterValue("Priority", ParameterUpdatePriority.Medium);

                result = await Agent.ExecuteCommandAsync(command);
            }

            return result;
        }
                
        public async Task<ExecuteResult> GetObdObject(string uniqueId)
        {
            ExecuteResult result = null;
            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("GetObject");

            var objectDictionary = command?.Device.ObjectDictionary;

            if (objectDictionary != null)
            {
                foreach (var parameter in objectDictionary.Parameters)
                {
                    if (parameter is Parameter parameterEntry)
                    {
                        if (parameterEntry.UniqueId == uniqueId)
                        {
                            command.SetParameterValue("Index", parameterEntry.Index);
                            command.SetParameterValue("SubIndex", parameterEntry.SubIndex);
                            command.SetParameterValue("Data", new byte[parameterEntry.DataType.SizeInBytes]);

                            var response = await Agent.ExecuteCommand(command);

                            if (response != null)
                            {
                                response.GetParameterValue("ErrorCode", ref lastErrorCode);
                                response.GetParameterValue("Result", ref commandResult);

                                var data = response.GetParameter("Data");

                                result = new ExecuteResult
                                {
                                    Result = commandResult, ErrorCode = lastErrorCode
                                };

                                if (data != null)
                                {
                                    result.Parameters.Add(data);
                                }
                            }

                            break;
                        }
                    }
                }
            }

            return result;
        }
        
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool finalize)
        {
            if (finalize)
            {
                Agent?.Dispose();
            }
        }

        public ParameterBase SearchParameter(string uniqueId)
        {
            return Device?.SearchParameter(uniqueId);
        }

        public ParameterBase SearchParameter(ushort index, byte subIndex)
        {
            return Device?.SearchParameter(index, subIndex);
        }

        public async Task<bool> WriteParameter(Parameter parameter)
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("SetObject");

            if (command != null)
            {
                command.SetParameterValue("Index", parameter.Index);
                command.SetParameterValue("SubIndex", parameter.SubIndex);

                parameter.GetValue(out byte[] data);

                command.SetParameterValue("Data", data);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<bool> LockDevice(EltraDevice device)
        {
            return await Agent.LockDevice(device);
        }

        public async Task<bool> UnlockDevice(EltraDevice device)
        {
            return await Agent.UnlockDevice(device);
        }

        public async Task<bool> CanLockDevice(EltraDevice device)
        {
            return await Agent.CanLockDevice(device);
        }

        public async Task<bool> IsDeviceLocked(EltraDevice device)
        {
            const long minUpdateInterval = 1000;
            
            if(!_deviceLockWatch.IsRunning || _deviceLockWatch.ElapsedMilliseconds > minUpdateInterval)
            {
                _isDeviceLocked = await Agent.IsDeviceLocked(device);

                if (!_deviceLockWatch.IsRunning)
                { 
                    _deviceLockWatch.Start();
                }
                else
                {
                    _deviceLockWatch.Restart();
                }
            }

            return _isDeviceLocked;
        }

        #endregion
    }
}

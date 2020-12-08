using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.History;
using EltraCommon.Contracts.Interfaces;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Users;
using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.Classes;
using EltraConnector.SyncAgent;

namespace EltraConnector.UserAgent
{
    internal class DeviceAgent : UserCloudAgent, ICloudConnector
    {
        #region private fields

        private static object _registeredParameterLocker = new object();

        private List<DeviceCommand> _deviceCommands;
        private List<RegisteredParameter> _registeredParameters;

        #endregion

        #region Constructors

        public DeviceAgent(string url, UserIdentity identity, uint updateInterval, uint timeout)
            : base(url, identity, updateInterval, timeout)
        {
        }

        public DeviceAgent(string url, string uuid, UserIdentity identity, uint updateInterval, uint timeout)
            : base(url, uuid, identity, updateInterval, timeout)
        {
        }

        internal DeviceAgent(SyncCloudAgent masterAgent, EltraDevice device, uint updateInterval, uint timeout)
            : base(masterAgent, device, updateInterval, timeout)
        {

        }
        
        #endregion
        
        #region Properties

        public List<DeviceCommand> DeviceCommands => _deviceCommands ?? (_deviceCommands = new List<DeviceCommand>());

        private List<RegisteredParameter> RegisteredParameters => _registeredParameters ?? (_registeredParameters = new List<RegisteredParameter>());

        #endregion

        #region Methods

        /// <summary>
        /// IsParameterRegistered
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
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

        public override async Task<DeviceCommand> GetDeviceCommand(EltraDevice device, string commandName)
        {
            var result = await base.GetDeviceCommand(device, commandName);

            if (result != null)
            {
                AddDeviceCommand(result);
            }

            return result;
        }

        private void AddDeviceCommand(DeviceCommand command)
        {
            DeviceCommands.Add(command);
        }

        public void Clear()
        {
            DeviceCommands.Clear();
        }

        private async Task<List<DeviceCommand>> AddDeviceCommands(EltraDevice device)
        {
            var result = new List<DeviceCommand>();
            var commands = await GetDeviceCommands(device);

            if (commands != null && commands.Count > 0)
            {
                result.AddRange(commands);
            }

            return result;
        }

        public async Task<Parameter> GetParameter(EltraDevice device, ushort index, byte subIndex)
        {
            Parameter result = null;

            if (device != null)
            {
                if (device.SearchParameter(index, subIndex) is Parameter parameterEntry)
                {
                    var parameterValue = await GetParameterValue(device, parameterEntry.Index, parameterEntry.SubIndex);

                    if (parameterValue != null)
                    {
                        parameterEntry.SetValue(parameterValue);
                    }

                    result = parameterEntry;
                }
                else
                {
                    result = await GetParameter(device.ChannelId, device.NodeId, index, subIndex);
                }

                if (result != null && result.Device == null)
                {
                    result.Device = device;
                }
            }

            return result;
        }

        public async Task<Parameter> GetParameter(EltraDevice device, string uniqueId)
        {
            Parameter result = null;

            if (device != null)
            {
                var parameterBase = device.SearchParameter(uniqueId);

                if (parameterBase is Parameter parameter)
                {
                    result = await GetParameter(device.ChannelId, device.NodeId, parameter.Index, parameter.SubIndex);
                }
                else
                {
                    result = await GetParameter(device.ChannelId, device.NodeId, parameterBase.Index, 0x0);
                }

                if (result != null)
                {
                    result.Device = device;
                }
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(EltraDevice device, ushort index, byte subIndex)
        {
            ParameterValue result = null;

            if (device != null)
            {
                result = await GetParameterValue(device.ChannelId, device.NodeId, index, subIndex);
            }

            return result;
        }

        public async Task<List<ParameterValue>> GetParameterValueHistory(EltraDevice device, string uniqueId, DateTime from, DateTime to)
        {
            List<ParameterValue> result = null;

            if (device != null)
            {
                result = await GetParameterValueHistory(device.ChannelId, device.NodeId, uniqueId, from, to);
            }

            return result;
        }

        public async Task<List<ParameterValue>> GetParameterValueHistory(EltraDevice device, ushort index, byte subIndex, DateTime from, DateTime to)
        {
            List<ParameterValue> result = null;

            if (device != null)
            {
                var parameter = device.SearchParameter(index, subIndex);

                if (parameter != null)
                {
                    result = await GetParameterValueHistory(device.ChannelId, device.NodeId, parameter.UniqueId, from, to);
                }
            }

            return result;
        }

        public async Task<ParameterValueHistoryStatistics> GetParameterValueHistoryStatistics(EltraDevice device, string uniqueId, DateTime from, DateTime to)
        {
            ParameterValueHistoryStatistics result = null;

            if (device != null)
            {
                result = await GetParameterValueHistoryStatistics(device.ChannelId, device.NodeId, uniqueId, from, to);
            }

            return result;
        }

        /// <summary>
        /// RegisterParameterUpdate
        /// </summary>
        /// <param name="device"></param>
        /// <param name="uniqueId"></param>
        /// <param name="priority"></param>
        /// <param name="waitForResult"></param>
        public bool RegisterParameterUpdate(EltraDevice device, string uniqueId, ParameterUpdatePriority priority = ParameterUpdatePriority.Low, bool waitForResult = false)
        {
#pragma warning disable 4014

            Task.Run(async () => {
                bool result = false;

                if (!string.IsNullOrEmpty(uniqueId))
                {
                    if (!IsParameterRegistered(uniqueId))
                    {
                        if (device != null)
                        {
                            if (device.SearchParameter(uniqueId) is Parameter parameterEntry)
                            {
                                AddToRegisteredParameters(uniqueId);

                                var command = await GetDeviceCommand(device, "RegisterParameterUpdate");

                                if (command != null)
                                {
                                    command.SetParameterValue("Index", parameterEntry.Index);
                                    command.SetParameterValue("SubIndex", parameterEntry.SubIndex);
                                    command.SetParameterValue("Priority", (int)priority);

                                    result = await ExecuteCommandAsync(command);
                                }

                                if (!result)
                                {
                                    RemoveFromRegisteredParameters(uniqueId);
                                
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

            }).ConfigureAwait(waitForResult);

#pragma warning restore 4014

            return true;
        }

        /// <summary>
        /// UnregisterParameterUpdate
        /// </summary>
        /// <param name="device"></param>
        /// <param name="uniqueId"></param>
        /// <param name="priority"></param>
        /// <param name="waitForResult"></param>
        public bool UnregisterParameterUpdate(EltraDevice device, string uniqueId, ParameterUpdatePriority priority = ParameterUpdatePriority.Low, bool waitForResult = false)
        {
            var t = Task.Run(async () =>
            {
                if (!string.IsNullOrEmpty(uniqueId))
                {
                    bool result = false;

                    if (FindRegisteredParameter(uniqueId, out var registeredParameter))
                    {
                        if (registeredParameter.InstanceCount <= 1)
                        {
                            RemoveFromRegisteredParameters(uniqueId);

                            if (device != null)
                            {
                                if (device.SearchParameter(uniqueId) is Parameter parameterEntry)
                                {
                                    var command = await GetDeviceCommand(device, "UnregisterParameterUpdate");

                                    if (command != null)
                                    {
                                        command.SetParameterValue("Index", parameterEntry.Index);
                                        command.SetParameterValue("SubIndex", parameterEntry.SubIndex);
                                        command.SetParameterValue("Priority", (int)priority);

                                        result = await ExecuteCommandAsync(command);
                                    }
                                }
                            }

                            if (!result)
                            {
                                AddToRegisteredParameters(uniqueId);

                                MsgLogger.WriteError($"{GetType().Name} - UnregisterParameterUpdate", $"parameter could't be unregistered - '{uniqueId}'");       
                            }
                            else
                            {
                                MsgLogger.WriteDebug($"{GetType().Name} - UnregisterParameterUpdate", $"unregistered parameter '{uniqueId}'");
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
            }).ConfigureAwait(waitForResult);

            return true;
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

        /// <summary>
        /// WriteParameter - Write parameter in cloud IoT service
        /// </summary>
        /// <param name="device">Eltra device instance</param>
        /// <param name="parameter">{Parameter} instance</param>
        /// <returns></returns>
        public async Task<bool> WriteParameter(EltraDevice device, Parameter parameter)
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetDeviceCommand(device, "SetObject");

            if (command != null)
            {
                command.SetParameterValue("Index", parameter.Index);
                command.SetParameterValue("SubIndex", parameter.SubIndex);

                parameter.GetValue(out byte[] data);

                command.SetParameterValue("Data", data);

                var responseCommand = await ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        /// <summary>
        /// UpdateDeviceDescriptionFile
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        protected override Task UpdateDeviceDescriptionFile(EltraDevice device)
        {
            device.CloudConnector = this;

            return base.UpdateDeviceDescriptionFile(device);
        }

        #endregion
    }
}

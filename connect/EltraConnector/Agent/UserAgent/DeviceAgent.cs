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
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Transport;
using EltraConnector.Agent.UserAgent.Cache;
using EltraConnector.SyncAgent;
using EltraConnector.Transport.Udp;
using EltraConnector.Transport.Ws;

namespace EltraConnector.UserAgent
{
    internal class DeviceAgent : UserCloudAgent, ICloudConnector
    {
        #region private fields

        private ParameterRegistrationCache _cache;

        private List<DeviceCommand> _deviceCommands;
        
        #endregion

        #region Constructors

        public DeviceAgent(IHttpClient httpClient, IUdpClient udpClient, IWebSocketClient webSocketClient, string url, string uuid, UserIdentity identity, uint updateInterval, uint timeout)
            : base(httpClient, udpClient, webSocketClient, url, uuid, identity, updateInterval, timeout)
        {  
        }

        internal DeviceAgent(IHttpClient httpClient, IUdpClient udpClient, SyncCloudAgent masterAgent, EltraDevice device, uint updateInterval, uint timeout)
            : base(httpClient, udpClient, masterAgent, device, updateInterval, timeout)
        {
            device.CloudConnector = this;
        }
        
        #endregion
        
        #region Properties

        public List<DeviceCommand> DeviceCommands => _deviceCommands ?? (_deviceCommands = new List<DeviceCommand>());

        private ParameterRegistrationCache ParameterRegistrationCache => _cache ?? (_cache = new ParameterRegistrationCache());

        #endregion

        #region Methods

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
                if(ParameterRegistrationCache.FindParameter(index, subIndex, out var registeredParameter))
                {
                    if(registeredParameter.CanUseCache)
                    {
                        var parameter = device.SearchParameter(index, subIndex) as XddParameter;

                        if (parameter != null)
                        {
                            result = parameter.ActualValue;
                        }
                    }
                    else
                    {
                        result = await GetParameterValue(device.ChannelId, device.NodeId, index, subIndex);

                        if (result != null)
                        {
                            registeredParameter.LastModified = DateTime.Now;
                        }
                    }
                }
                else
                {
                    result = await GetParameterValue(device.ChannelId, device.NodeId, index, subIndex);
                }
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
            if (!string.IsNullOrEmpty(uniqueId) && device != null && device.SearchParameter(uniqueId) is XddParameter parameterEntry)
            {   
                bool result = false;
                int instanceCount = 0;

                if (!ParameterRegistrationCache.IsParameterRegistered(uniqueId))
                {
                    instanceCount = ParameterRegistrationCache.AddParameter(uniqueId, parameterEntry.Index, parameterEntry.SubIndex, parameterEntry.Flags);

                    Task.Run(async () => {

                        var command = await GetDeviceCommand(device, "RegisterParameterUpdate");

                        if (command != null)
                        {
                            command.SetParameterValue("Index", parameterEntry.Index);
                            command.SetParameterValue("SubIndex", parameterEntry.SubIndex);
                            command.SetParameterValue("Priority", (int)priority);

                            result = await ExecuteCommandAsync(command);

                            if (!result)
                            {
                                instanceCount = ParameterRegistrationCache.RemoveParameter(uniqueId);

                                MsgLogger.WriteError($"{GetType().Name} - RegisterParameterUpdate", $"parameter could't be registered - '{uniqueId}'");
                            }
                            else
                            {
                                MsgLogger.WriteDebug($"{GetType().Name} - RegisterParameterUpdate", $"registered parameter '{uniqueId}', instance count = {instanceCount}");
                            }
                        }
                    }).ConfigureAwait(waitForResult);
                }
                else
                {
                    if (ParameterRegistrationCache.IncreaseCounter(uniqueId, out var registeredParameter))
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - RegisterParameterUpdate", $"register parameter '{uniqueId}', instance count = {registeredParameter.InstanceCount}");
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - RegisterParameterUpdate", $"register parameter '{uniqueId}', instance count = {instanceCount}");
                    }
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - RegisterParameterUpdate", $"parameter '{uniqueId}' not found");
            }

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
            if (device != null && !string.IsNullOrEmpty(uniqueId) && device.SearchParameter(uniqueId) is Parameter parameterEntry)
            {
                bool result = false;

                if (ParameterRegistrationCache.CanUnregister(uniqueId, out var registeredParameter))
                {       
                    Task.Run(async () =>
                    {
                        var command = await GetDeviceCommand(device, "UnregisterParameterUpdate");

                        if (command != null)
                        {
                            command.SetParameterValue("Index", parameterEntry.Index);
                            command.SetParameterValue("SubIndex", parameterEntry.SubIndex);
                            command.SetParameterValue("Priority", (int)priority);

                            result = await ExecuteCommandAsync(command);

                            if (!result)
                            {
                                ParameterRegistrationCache.AddParameter(registeredParameter);

                                MsgLogger.WriteError($"{GetType().Name} - UnregisterParameterUpdate", $"parameter could't be unregistered - '{uniqueId}'");
                            }
                            else
                            {
                                MsgLogger.WriteDebug($"{GetType().Name} - UnregisterParameterUpdate", $"unregistered parameter '{uniqueId}'");
                            }
                        }
                    }).ConfigureAwait(waitForResult);                        
                }
                else
                {
                    registeredParameter?.Release();

                    MsgLogger.WriteDebug($"{GetType().Name} - UnregisterParameterUpdate", $"unregister parameter '{uniqueId}', instance count = {registeredParameter?.InstanceCount}");

                    result = true;
                }                
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - UnregisterParameterUpdate", $"unregister: cannot find registered parameter '{uniqueId}'");
            }
        
            return true;
        }

        /// <summary>
        /// WriteParameter - Write parameter in cloud IoT service
        /// </summary>
        /// <param name="device">Eltra device instance</param>
        /// <param name="parameter">{Parameter} instance</param>
        /// <returns></returns>
        public async Task<bool> WriteParameter(EltraDevice device, Parameter parameter)
        {
            const string methodName = "WriteParameter";
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetDeviceCommand(device, "SetObject");

            if (command != null)
            {
                if (parameter != null)
                {
                    command.SetParameterValue("Index", parameter.Index);
                    command.SetParameterValue("SubIndex", parameter.SubIndex);

                    if (parameter.GetValue(out byte[] data))
                    {
                        command.SetParameterValue("Data", data);

                        var responseCommand = await ExecuteCommand(command);

                        responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                        responseCommand?.GetParameterValue("Result", ref result);

                        if (result && ParameterRegistrationCache.FindParameter(parameter.UniqueId, out var registeredParameter))
                        {
                            registeredParameter.Reset();
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"Cannot get parameter {parameter.UniqueId} value");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"parameter not found!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"command not found!");
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

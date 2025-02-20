using System;
using System.Threading;
using System.Threading.Tasks;
using EltraConnector.Classes;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using System.Collections.Generic;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;
using EltraCommon.Contracts.History;
using EltraCommon.Contracts.Parameters.Events;
using EltraCommon.Contracts.Channels;
using EltraConnector.Transport.Ws;
using EltraCommon.Transport;
using EltraConnector.Transport.Udp;

namespace EltraConnector.UserAgent.Vcs
{
    /// <summary>
    /// DeviceVcs class
    /// </summary>
    public class DeviceVcs : IDisposable 
    {
        #region Private fields

        private const int DefaultTimeout = 30000;

        private EltraDevice _deviceNode;
        private Channel _deviceChannel;

        #endregion

        #region Constructors

        /// <summary>
        /// DeviceVcs
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="udpClient"></param>
        /// <param name="webSocketClient"></param>
        /// <param name="url"></param>
        /// <param name="uuid"></param>
        /// <param name="identity"></param>
        /// <param name="updateInterval"></param>
        /// <param name="timeout"></param>
        public DeviceVcs(IHttpClient httpClient, IUdpClient udpClient, IWebSocketClient webSocketClient, string url, string uuid, UserIdentity identity, uint updateInterval, uint timeout)
        {
            if (identity != null)
            {
                _deviceNode = new EltraDevice() { ChannelId = uuid };

                Timeout = DefaultTimeout;

                Agent = new DeviceAgent(httpClient, udpClient, webSocketClient, url, uuid, identity, updateInterval, timeout);

                Agent.ParameterValueChanged += OnParameterValueChanged;

                _deviceNode.CloudConnector = Agent;
            }
        }

        internal DeviceVcs(IHttpClient httpClient, IUdpClient udpClient, SyncCloudAgent masterAgent, EltraDevice deviceNode, uint updateInterval, uint timeout)
        {
            if (deviceNode != null)
            {
                _deviceNode = deviceNode;

                Timeout = DefaultTimeout;

                Agent = new DeviceAgent(httpClient, udpClient, masterAgent, deviceNode, updateInterval, timeout);

                Agent.ParameterValueChanged += OnParameterValueChanged;

                _deviceNode.CloudConnector = Agent;
            }
        }

        internal DeviceVcs(DeviceAgent agent, EltraDevice deviceNode)
        {
            Timeout = DefaultTimeout;

            if (deviceNode != null)
            {
                _deviceNode = deviceNode;

                if (agent != null)
                {
                    Agent = agent;

                    Agent.ParameterValueChanged += OnParameterValueChanged;

                    _deviceNode.CloudConnector = Agent;
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Device Channel
        /// </summary>
        public Channel DeviceChannel
        {
            get => _deviceChannel;
            set
            {
                _deviceChannel = value;
                OnDeviceChannelChanged();
            }
        }

        /// <summary>
        /// Device node instance
        /// </summary>
        public EltraDevice Device
        {
            get => _deviceNode;
            set
            {
                _deviceNode = value;
                OnDeviceChanged();
            }
        }

        internal DeviceAgent Agent { get; }

        /// <summary>
        /// Timeout
        /// </summary>
        public int Timeout { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Device changed event
        /// </summary>
        public event EventHandler DeviceChanged;

        /// <summary>
        /// Device channel changed
        /// </summary>
        public event EventHandler<Channel> DeviceChannelChanged;

        #endregion

        #region Events handling

        private void OnParameterValueChanged(object sender, ParameterValueChangedEventArgs e)
        {
            var objectDictionary = Device?.ObjectDictionary;
            var channelId = Device?.ChannelId;

            if (objectDictionary != null && e.ChannelId == channelId)
            {
                var parameterBase = objectDictionary.SearchParameter(e.Index, e.SubIndex);

                if(parameterBase is Parameter parameterEntry)
                {
                    parameterEntry.ActualValue = e.ParameterValue;
                }
            }
        }

        /// <summary>
        /// OnDeviceChanged
        /// </summary>
        protected virtual void OnDeviceChanged()
        {
            if(Agent!=null && Device!=null)
            {
                Device.CloudConnector = Agent;
            }

            DeviceChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// OnDeviceChannelChanged
        /// </summary>
        protected virtual void OnDeviceChannelChanged()
        {
            DeviceChannelChanged?.Invoke(this, DeviceChannel);
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

        /// <summary>
        /// GetVcsCommand - get vcs command
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        protected async Task<DeviceCommand> GetVcsCommand(string commandName)
        {
            DeviceCommand result = FindBufferedVcsCommand(commandName);
            
            if (result == null && Device != null)
            {
                result = await Agent.GetDeviceCommand(_deviceNode, commandName);

                var deviceNode = result?.Device;
                var device = deviceNode;

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
                device.StatusChanged += (sender, args) => 
                { 
                    if(device.Status == DeviceStatus.Ready)
                    {
                        token.Set();
                    }
                };
                
                if(!await device.ReadDeviceDescriptionFile())
                {
                    MsgLogger.WriteError($"{GetType().Name} - DownloadObjectDictionary", "read device description file failed!");

                    device.Status = DeviceStatus.Ready;

                    token.Set();
                }
            });

            token.WaitOne(Timeout);
        }

        /// <summary>
        /// UpdateParameterValue
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public async Task<bool> UpdateParameterValue(string uniqueId)
        {
            bool result = false;

            if (Device != null && Device.SearchParameter(uniqueId) is Parameter parameterEntry)
            {
                var parameterValue = await Agent.GetParameterValue(Device.ChannelId, Device.NodeId,
                    parameterEntry.Index, parameterEntry.SubIndex);

                result = parameterEntry.SetValue(parameterValue);
            }

            return result;
        }

        /// <summary>
        /// UpdateParameterValue
        /// </summary>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <returns></returns>
        public async Task<bool> UpdateParameterValue(ushort index, byte subIndex)
        {
            bool result = false;

            if (Device != null && Device.SearchParameter(index, subIndex) is Parameter parameterEntry)
            {
                var parameterValue = await Agent.GetParameterValue(Device.ChannelId, Device.NodeId, index, subIndex);

                result = parameterEntry.SetValue(parameterValue);
            }

            return result;
        }

        /// <summary>
        /// GetParameterValue
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public async Task<ParameterValue> GetParameterValue(string uniqueId)
        {
            ParameterValue result = null;

            if (Device != null && Device.SearchParameter(uniqueId) is Parameter parameterEntry)
            {
                result = await Agent.GetParameterValue(Device, parameterEntry.Index, parameterEntry.SubIndex);
            }

            return result;
        }

        /// <summary>
        /// GetParameterValue
        /// </summary>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <returns></returns>
        public async Task<ParameterValue> GetParameterValue(ushort index, byte subIndex)
        {
            ParameterValue result = null;

            if (Device != null)
            {
                result = await Agent.GetParameterValue(Device, index, subIndex);
            }

            return result;
        }

        /// <summary>
        /// GetParameter
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public async Task<Parameter> GetParameter(string uniqueId)
        {
            Parameter result = null;

            if (Device != null && Device.SearchParameter(uniqueId) is Parameter parameterEntry)
            {
                var parameterValue = await Agent.GetParameterValue(Device, parameterEntry.Index, parameterEntry.SubIndex);

                if (parameterValue != null && parameterEntry.SetValue(parameterValue))
                {
                    result = parameterEntry;
                }
            }

            return result;
        }

        /// <summary>
        /// GetParameter
        /// </summary>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <returns></returns>
        public async Task<Parameter> GetParameter(ushort index, byte subIndex)
        {
            Parameter result = null;

            if (Device != null)
            {
                result = await Agent.GetParameter(Device, index, subIndex);
            }

            return result;
        }

        /// <summary>
        /// GetParameterValueHistoryStatistics
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task<ParameterValueHistoryStatistics> GetParameterValueHistoryStatistics(string uniqueId, DateTime from, DateTime to)
        {
            ParameterValueHistoryStatistics result = null;

            if (Agent != null && Device != null)
            {
                result = await Agent.GetParameterValueHistoryStatistics(Device, uniqueId, from, to);
            }

            return result;
        }

        /// <summary>
        /// GetParameterValueHistory
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task<List<ParameterValue>> GetParameterValueHistory(string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();

            if (Agent != null && Device != null)
            {
                result = await Agent.GetParameterValueHistory(Device, uniqueId, from, to);
            }

            return result;
        }

        /// <summary>
        /// GetParameterValueHistory
        /// </summary>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task<List<ParameterValue>> GetParameterValueHistory(ushort index, byte subIndex, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();

            if (Device != null && Device.SearchParameter(index, subIndex) is Parameter parameterEntry)
            {
                result = await Agent.GetParameterValueHistory(Device, parameterEntry.UniqueId, from, to);
            }

            return result;
        }

        /// <summary>
        /// RegisterParameterUpdate
        /// </summary>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <returns></returns>
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

        /// <summary>
        /// UnregisterParameterUpdate
        /// </summary>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <returns></returns>
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

        /// <summary>
        /// GetObdObject
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public async Task<ExecuteResult> GetObdObject(string uniqueId)
        {
            ExecuteResult result = null;
            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("GetObject");

            var objectDictionary = command?.Device?.ObjectDictionary;

            if (objectDictionary != null)
            {
                foreach (var parameter in objectDictionary.Parameters)
                {
                    if (parameter is Parameter parameterEntry && parameterEntry.UniqueId == uniqueId)
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
                                Result = commandResult,
                                ErrorCode = lastErrorCode
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

            return result;
        }
        
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="finalize"></param>
        protected virtual void Dispose(bool finalize)
        {
            if (finalize)
            {
                Agent?.Dispose();
            }
        }

        /// <summary>
        /// Search parameter by parameter uniqueId
        /// </summary>
        /// <param name="uniqueId">parameter uniqueId</param>
        /// <returns></returns>
        public ParameterBase SearchParameter(string uniqueId)
        {
            return Device?.SearchParameter(uniqueId);
        }

        /// <summary>
        /// Search parameter by parameter index and subindex
        /// </summary>
        /// <param name="index">parameter index</param>
        /// <param name="subIndex">parameter subindex</param>
        /// <returns></returns>
        public ParameterBase SearchParameter(ushort index, byte subIndex)
        {
            return Device?.SearchParameter(index, subIndex);
        }

        /// <summary>
        /// WriteParameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
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

        #endregion
    }
}

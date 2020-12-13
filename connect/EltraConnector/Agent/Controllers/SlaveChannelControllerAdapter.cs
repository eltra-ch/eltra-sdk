using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraConnector.Controllers.Base;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.Transport.Ws;
using EltraCommon.Contracts.History;
using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.DeviceDescription;
using EltraConnector.Controllers.Device;
using EltraConnector.Agent.Controllers.Device;

namespace EltraConnector.Agent.Controllers
{
    class SlaveChannelControllerAdapter : ChannelControllerAdapter
    {
        #region Private fields

        private DeviceControllerAdapter _deviceControllerAdapter;
        private bool _useWebSockets;

        #endregion

        #region Constructors

        public SlaveChannelControllerAdapter(string url, UserIdentity identity, uint updateInterval, uint timeout)
            : base(url, identity, updateInterval, timeout)
        {   
        }

        public SlaveChannelControllerAdapter(string url, string uuid, UserIdentity identity, uint updateInterval, uint timeout)
            : base(url, uuid, identity, updateInterval, timeout)
        {   
        }

        #endregion

        #region Properties

        private DeviceControllerAdapter DeviceAdapter => _deviceControllerAdapter ?? (_deviceControllerAdapter = CreateDeviceController());

        public bool UseWebSockets 
        { 
            get => _useWebSockets; 
            set 
            {
                if (_useWebSockets != value)
                {
                    _useWebSockets = value;

                    OnUseWebSocketsChanged();
                }
            } 
        }

        #endregion

        #region Events handling

        private async void OnUseWebSocketsChanged()
        {
            if (UseWebSockets)
            {
                var wsConnectionManager = new WsConnectionManager() { HostUrl = Url };

                WsConnectionManager = wsConnectionManager;

                DeviceAdapter.WsConnectionManager = wsConnectionManager;
            }
            else
            {
                if(WsConnectionManager!=null)
                {
                    await WsConnectionManager.DisconnectAll();
                }

                WsConnectionManager = null;
            }
        }

        #endregion

        #region Methods

        private DeviceControllerAdapter CreateDeviceController()
        {
            var result = new SlaveDeviceControllerAdapter(Url, Channel, User.Identity);
            
            return result;
        }
                        
        public async Task<List<EltraDevice>> GetDeviceNodes(Channel channel)
        {
            return await DeviceAdapter.GetDeviceNodes(channel.Id);
        }
        
        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDevice device)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.GetDeviceCommands(device);
        }

        public async Task<DeviceCommand> GetDeviceCommand(EltraDevice device, string commandName)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.GetDeviceCommand(device, commandName);
        }

        public async Task<bool> PushCommand(DeviceCommand command, string uuid, ExecCommandStatus status)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.PushCommand(command, uuid, status);
        }

        public async Task<List<ExecuteCommand>> PopCommands(EltraDevice device, ExecCommandStatus status)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.PullCommands(device, status);
        }

        public async Task<ExecuteCommand> PopCommand(string commandUuid, EltraDevice device, ExecCommandStatus status)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.PopCommand(commandUuid, device, status);
        }

        public async Task<bool> SetCommandStatus(ExecuteCommand executeCommand, ExecCommandStatus status)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.SetCommandStatus(executeCommand, status);
        }

        public async Task<bool> SetCommandStatus(ExecuteCommandStatus executeCommandStatus)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.SetCommandStatus(executeCommandStatus);
        }

        public async Task<ExecuteCommandStatus> GetCommandStatus(ExecuteCommand executeCommand)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.GetCommandStatus(ChannelId, executeCommand);
        }

        public Task<DeviceDescriptionPayload> DownloadDeviceDescription(string channelId, DeviceVersion deviceVersion)
        {
            return DeviceAdapter.DownloadDeviceDescription(channelId, deviceVersion);
        }

        public Task<DeviceDescriptionIdentity> GetDeviceDescriptionIdentity(string channelId, DeviceVersion deviceVersion)
        {
            return DeviceAdapter.GetDeviceDescriptionIdentity(channelId, deviceVersion);
        }

        public Task<Parameter> GetParameter(string sessionUuid, int nodeId, ushort index, byte subIndex)
        {
            return DeviceAdapter.GetParameter(sessionUuid, nodeId, index, subIndex);
        }

        public async Task<ParameterValue> GetParameterValue(string sessionUuid, int nodeId, ushort index, byte subIndex)
        {
            return await DeviceAdapter.GetParameterValue(sessionUuid, nodeId, index, subIndex);
        }
        
        public async Task<List<ParameterValue>> GetParameterHistory(string sessionUuid, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            return await DeviceAdapter.GetParameterHistory(sessionUuid, nodeId, uniqueId, from, to);
        }

        public async Task<ParameterValueHistoryStatistics> GetParameterHistoryStatistics(string channelId, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            return await DeviceAdapter.GetParameterHistoryStatistics(channelId, nodeId, uniqueId, from, to);
        }

        #endregion
    }
}

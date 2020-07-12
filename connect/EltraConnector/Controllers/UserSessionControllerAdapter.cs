using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EltraConnector.Controllers.Base;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Sessions;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.Transport.Ws;
using EltraCommon.Contracts.Node;

namespace EltraConnector.Controllers
{
    class UserSessionControllerAdapter : SessionControllerAdapter
    {
        #region Private fields

        private DeviceControllerAdapter _deviceControllerAdapter;
        private bool _useWebSockets;

        #endregion

        #region Constructors

        public UserSessionControllerAdapter(string url, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url, authData, updateInterval, timeout)
        {   
        }

        public UserSessionControllerAdapter(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url, uuid, authData, updateInterval, timeout)
        {   
        }

        #endregion

        #region Properties

        public string Uuid => Session.Uuid;

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
            DeviceControllerAdapter result = new DeviceControllerAdapter(Url, Session);
            
            return result;
        }
                        
        public async Task<List<EltraDeviceNode>> GetDeviceNodes(Session session, UserAuthData authData)
        {
            return await DeviceAdapter.GetDeviceNodes(session.Uuid, authData);
        }
        
        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDeviceNode device)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.GetDeviceCommands(device);
        }

        public async Task<DeviceCommand> GetDeviceCommand(EltraDeviceNode device, string commandName)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.GetDeviceCommand(device, commandName);
        }

        public async Task<bool> PushCommand(DeviceCommand command, string uuid, ExecCommandStatus status)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.PushCommand(command, uuid, status);
        }

        public async Task<List<ExecuteCommand>> PopCommands(EltraDeviceNode device, ExecCommandStatus status)
        {
            var deviceCommandsAdapter = DeviceAdapter.DeviceCommandsAdapter;

            return await deviceCommandsAdapter.PullCommands(device, status);
        }

        public async Task<ExecuteCommand> PopCommand(string commandUuid, EltraDeviceNode device, ExecCommandStatus status)
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

            return await deviceCommandsAdapter.GetCommandStatus(Uuid, executeCommand);
        }

        public async Task<Parameter> GetParameter(int nodeId, ushort index, byte subIndex)
        {
            return await DeviceAdapter.GetParameter(nodeId, index, subIndex);
        }

        public async Task<ParameterValue> GetParameterValue(int nodeId, ushort index, byte subIndex)
        {
            return await DeviceAdapter.GetParameterValue(nodeId, index, subIndex);
        }
        
        public async Task<List<ParameterValue>> GetParameterHistory(int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            return await DeviceAdapter.GetParameterHistory(nodeId, uniqueId, from, to);
        }

        #endregion
    }
}

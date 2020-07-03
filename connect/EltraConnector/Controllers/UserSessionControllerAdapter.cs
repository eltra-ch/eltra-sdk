using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EltraConnector.Controllers.Base;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Sessions;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Contracts.Parameters;
using EltraConnector.Transport.Ws;

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
                        
        public async Task<List<EltraDevice>> GetSessionDevices(Session session, UserAuthData authData)
        {
            return await DeviceAdapter.GetSessionDevices(session.Uuid, authData);
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

            return await deviceCommandsAdapter.PopCommands(device, status);
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

            return await deviceCommandsAdapter.GetCommandStatus(Uuid, executeCommand);
        }

        public async Task<Parameter> GetParameter(ulong serialNumber, ushort index, byte subIndex)
        {
            return await DeviceAdapter.GetParameter(serialNumber, index, subIndex);
        }

        public async Task<ParameterValue> GetParameterValue(ulong serialNumber, ushort index, byte subIndex)
        {
            return await DeviceAdapter.GetParameterValue(serialNumber, index, subIndex);
        }
        
        public async Task<List<ParameterValue>> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to)
        {
            return await DeviceAdapter.GetParameterHistory(serialNumber, uniqueId, from, to);
        }

        public async Task<List<ParameterUniqueIdValuePair>> GetParameterHistoryPair(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            return await DeviceAdapter.GetParameterHistoryPair(serialNumber, uniqueId1, uniqueId2, from, to);
        }


        public async Task<bool> LockDevice(EltraDevice device)
        {
            return await DeviceAdapter.LockDevice(Uuid, device);
        }

        public async Task<bool> UnlockDevice(EltraDevice device)
        {
            return await DeviceAdapter.UnlockDevice(Uuid, device);
        }

        public async Task<bool> CanLockDevice(EltraDevice device)
        {
            return await DeviceAdapter.CanLockDevice(Uuid, device);
        }

        public async Task<bool> IsDeviceLocked(EltraDevice device)
        {
            return await DeviceAdapter.IsDeviceLocked(Uuid, device);
        }

        #endregion
    }
}

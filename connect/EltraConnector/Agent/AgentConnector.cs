using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Sessions;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EltraConnector.Agent
{
    public class AgentConnector
    {
        #region Private fields

        private DeviceAgent _deviceAgent;
        private uint _updateInterval;
        private uint _timeout;
        private readonly List<DeviceVcs> _vcsList = new List<DeviceVcs>();

        #endregion

        #region Constructors

        public AgentConnector()
        {
            Host = "https://eltra.ch";
            AuthData = new UserAuthData() { Login = "agent@eltra.ch", Password = "1234" };
            _updateInterval = 60;
            _timeout = 180;
        }

        #endregion

        #region Properties

        public string Host { get; set; }

        public UserAuthData AuthData { get; set; }

        #endregion

        #region Methods

        private DeviceVcs FindVcs(EltraDevice device)
        {
            DeviceVcs result = null;

            if (device != null)
            {
                foreach (var vcs in _vcsList)
                {
                    var vcsIdent1 = vcs?.Device?.Identification;
                    var vcsIdent2 = device?.Identification;

                    if (vcsIdent1 !=null && vcsIdent2 != null &&
                        vcsIdent1.SerialNumber == vcsIdent2.SerialNumber)
                    {
                        result = vcs;
                        break;
                    }
                }
            }

            return result;
        }

        public async Task<List<EltraDevice>> GetDevices(UserAuthData deviceAuth)
        {
            var result = new List<EltraDevice>();

            _vcsList.Clear();
            _deviceAgent = new DeviceAgent(Host, AuthData, _updateInterval, _timeout);

            var sessionsDevices = await _deviceAgent.GetDevices(deviceAuth);

            foreach (var sessionDevice in sessionsDevices.SessionDevices)
            {
                var session = sessionDevice.Session;

                foreach (var device in sessionDevice.Devices)
                {
                    if (FindVcs(device) == null)
                    {
                        _vcsList.Add(new DeviceVcs(_deviceAgent, device));
                    }

                    result.Add(device);
                }
            }

            return result;
        }

        public async Task<SessionsDevices> GetSessionDevices(UserAuthData deviceAuth)
        {
            var result = new SessionsDevices();

            _vcsList.Clear();
            _deviceAgent = new DeviceAgent(Host, AuthData, _updateInterval, _timeout);

            var sessionsDevices = await _deviceAgent.GetDevices(deviceAuth);

            foreach (var sessionDevice in sessionsDevices.SessionDevices)
            {
                Session session = sessionDevice.Session;

                foreach (var device in sessionDevice.Devices)
                {
                    _vcsList.Add(new DeviceVcs(_deviceAgent, device));

                    result.Add(sessionDevice);
                }
            }

            return result;
        }

        private DeviceVcs SearchDeviceVcs(EltraDevice device)
        {
            DeviceVcs result = null;

            foreach (var vcs in _vcsList)
            {
                if(vcs.Device == device)
                {
                    result = vcs;
                    break;
                }
            }

            return result;
        }

        public ParameterBase SearchParameter(EltraDevice device, ushort index, byte subIndex)
        {
            ParameterBase result = null;
            
            var vcs = SearchDeviceVcs(device);

            if(vcs!=null)
            {
                result = vcs.SearchParameter(index, subIndex);
            }

            return result;
        }

        public ParameterBase SearchParameter(EltraDevice device, string uniqueId)
        {
            ParameterBase result = null;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = vcs.SearchParameter(uniqueId);
            }

            return result;
        }

        public async Task<Parameter> GetParameter(EltraDevice device, string uniqueId)
        {
            Parameter result = null;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.GetParameter(uniqueId);
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(EltraDevice device, string uniqueId)
        {
            ParameterValue result = null;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.GetParameterValue(uniqueId);
            }

            return result;
        }

        public async Task<bool> WriteParameter(EltraDevice device, Parameter parameter)
        {
            bool result = false;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.WriteParameter(parameter);
            }

            return result;
        }

        public async Task<bool> IsDeviceLocked(EltraDevice device)
        {
            bool result = false;
            
            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.IsDeviceLocked(device);
            }

            return result;
        }

        public async Task<bool> CanLockDevice(EltraDevice device)
        {
            bool result = false;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.CanLockDevice(device);
            }

            return result;
        }

        public async Task<bool> LockDevice(EltraDevice device)
        {
            bool result = false;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.LockDevice(device);
            }

            return result;
        }

        public async Task<bool> UnlockDevice(EltraDevice device)
        {
            bool result = false;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.UnlockDevice(device);
            }

            return result;
        }

        public void RegisterParameterUpdate(EltraDevice device, string uniqueId, ParameterUpdatePriority priority = ParameterUpdatePriority.Low)
        {
            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                vcs.RegisterParameterUpdate(uniqueId, priority);
            }
        }

        public void RegisterParameterUpdate(EltraDevice device, ushort index, byte subIndex, ParameterUpdatePriority priority = ParameterUpdatePriority.Low)
        {
            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                var parameter = vcs.SearchParameter(index, subIndex);

                if (parameter != null)
                {
                    vcs.RegisterParameterUpdate(parameter.UniqueId, priority);
                }
            }
        }

        public void UnregisterParameterUpdate(EltraDevice device, string uniqueId)
        {
            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                vcs.UnregisterParameterUpdate(uniqueId);
            }
        }

        public void UnregisterParameterUpdate(EltraDevice device, ushort index, byte subIndex)
        {
            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                var parameter = vcs.SearchParameter(index, subIndex);

                if (parameter != null)
                {
                    vcs.UnregisterParameterUpdate(parameter.UniqueId);
                }
            }
        }

        public async Task<DeviceCommand> GetDeviceCommand(EltraDevice device, string commandName)
        {
            DeviceCommand result = null;

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.GetDeviceCommand(device, commandName);
            }

            return result;
        }

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDevice device)
        {
            List<DeviceCommand> result = new List<DeviceCommand>();

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.GetDeviceCommands(device);
            }

            return result;
        }

        public async Task<List<ParameterValue>> GetParameterHistory(EltraDevice device, string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();
            
            if (_deviceAgent!=null)
            {
                result = await _deviceAgent.GetParameterHistory(device, uniqueId, from, to);
            }

            return result;
        }

        public async Task<List<ParameterUniqueIdValuePair>> GetParameterHistoryPair(EltraDevice device, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            var result = new List<ParameterUniqueIdValuePair>();

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.GetParameterHistoryPair(device, uniqueId1, uniqueId2, from, to);
            }

            return result;
        }

        public async Task<DeviceCommand> ExecuteCommand(DeviceCommand command)
        {
            DeviceCommand result = null;

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.ExecuteCommand(command);
            }

            return result;
        }

        #endregion
    }
}

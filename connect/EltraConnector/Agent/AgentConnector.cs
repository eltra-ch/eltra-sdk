using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Node;
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

        private DeviceVcs FindVcs(EltraDeviceNode device)
        {
            DeviceVcs result = null;

            if (device != null)
            {
                foreach (var vcs in _vcsList)
                {
                    var vcsDevice = vcs.Device;
                    
                    if (vcsDevice !=null &&
                        vcsDevice.NodeId == device.NodeId)
                    {
                        result = vcs;
                        break;
                    }
                }
            }

            return result;
        }

        public async Task<List<EltraDeviceNode>> GetDevices(UserAuthData deviceAuth)
        {
            var result = new List<EltraDeviceNode>();

            _vcsList.Clear();
            _deviceAgent = new DeviceAgent(Host, AuthData, _updateInterval, _timeout);

            var sessionsDevices = await _deviceAgent.GetDevices(deviceAuth);

            foreach (var sessionDevices in sessionsDevices)
            {
                var session = sessionDevices.Session;

                foreach (var deviceNode in sessionDevices.DeviceNodeList)
                {
                    var device = deviceNode;

                    if (FindVcs(device) == null)
                    {
                        _vcsList.Add(new DeviceVcs(_deviceAgent, deviceNode));
                    }

                    result.Add(deviceNode);
                }
            }

            return result;
        }

        public async Task<List<EltraDeviceNodeList>> GetSessionDevices(UserAuthData deviceAuth)
        {
            var result = new List<EltraDeviceNodeList>();

            _vcsList.Clear();
            _deviceAgent = new DeviceAgent(Host, AuthData, _updateInterval, _timeout);

            var sessionDeviceNodeList = await _deviceAgent.GetDevices(deviceAuth);

            if (sessionDeviceNodeList != null)
            {
                foreach (var deviceNodeList in sessionDeviceNodeList)
                {
                    foreach (var deviceNode in deviceNodeList.DeviceNodeList)
                    {
                        _vcsList.Add(new DeviceVcs(_deviceAgent, deviceNode));
                    }
                }

                result = sessionDeviceNodeList;
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

        public async Task<DeviceCommand> GetDeviceCommand(EltraDeviceNode device, string commandName)
        {
            DeviceCommand result = null;

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.GetDeviceCommand(device, commandName);
            }

            return result;
        }

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDeviceNode device)
        {
            List<DeviceCommand> result = new List<DeviceCommand>();

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.GetDeviceCommands(device);
            }

            return result;
        }

        public async Task<List<ParameterValue>> GetParameterHistory(EltraDeviceNode device, string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();
            
            if (_deviceAgent!=null)
            {
                result = await _deviceAgent.GetParameterHistory(device, uniqueId, from, to);
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

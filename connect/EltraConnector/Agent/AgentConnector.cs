using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.Contracts.Users;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
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
        private List<DeviceVcs> _vcsList;

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

        public async Task<List<EltraDevice>> GetDevices(UserAuthData deviceAuth)
        {
            var result = new List<EltraDevice>();

            _vcsList.Clear();
            _deviceAgent = new DeviceAgent(Host, AuthData, _updateInterval, _timeout);

            var sessionDevices = await _deviceAgent.GetDevices(deviceAuth);

            foreach(var sessionDevice in sessionDevices)
            {
                EltraDevice device = sessionDevice.Item2;

                _vcsList.Add(new DeviceVcs(_deviceAgent, device));

                result.Add(sessionDevice.Item2);
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

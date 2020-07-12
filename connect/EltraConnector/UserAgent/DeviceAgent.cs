using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Node;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.SyncAgent;

namespace EltraConnector.UserAgent
{
    internal class DeviceAgent : UserCloudAgent
    {
        #region private fields

        private List<DeviceCommand> _deviceCommands;
        
        #endregion

        #region Constructors

        public DeviceAgent(string url, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url, authData, updateInterval, timeout)
        {
        }

        public DeviceAgent(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url, uuid, authData, updateInterval, timeout)
        {
        }

        public DeviceAgent(SyncCloudAgent masterAgent, EltraDeviceNode device, uint updateInterval, uint timeout)
            : base(masterAgent, device, updateInterval, timeout)
        {

        }
        
        #endregion
        
        #region Properties

        public List<DeviceCommand> DeviceCommands => _deviceCommands ?? (_deviceCommands = new List<DeviceCommand>());

        #endregion

        #region Methods

        public override async Task<DeviceCommand> GetDeviceCommand(EltraDeviceNode device, string commandName)
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

        private async Task<List<DeviceCommand>> AddDeviceCommands(EltraDeviceNode device)
        {
            var result = new List<DeviceCommand>();
            var commands = await GetDeviceCommands(device);

            if (commands != null && commands.Count > 0)
            {
                result.AddRange(commands);
            }

            return result;
        }

        public async Task<Parameter> GetParameter(EltraDeviceNode device, ushort index, byte subIndex)
        {
            Parameter result = null;

            if (device != null)
            {
                result = await GetParameter(device.NodeId, index, subIndex);
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(EltraDeviceNode device, ushort index, byte subIndex)
        {
            ParameterValue result = null;

            if (device != null)
            {
                result = await GetParameterValue(device.NodeId, index, subIndex);
            }

            return result;
        }

        public async Task<List<ParameterValue>> GetParameterHistory(EltraDeviceNode device, string uniqueId, DateTime from, DateTime to)
        {
            List<ParameterValue> result = null;

            if (device != null)
            {
                result = await GetParameterHistory(device.NodeId, uniqueId, from, to);
            }

            return result;
        }

        #endregion
    }
}

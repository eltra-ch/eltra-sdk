using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.Contracts.Users;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
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

        public DeviceAgent(SyncCloudAgent masterAgent, EltraDevice device, uint updateInterval, uint timeout)
            : base(masterAgent, device, updateInterval, timeout)
        {

        }
        
        #endregion
        
        #region Properties

        public List<DeviceCommand> DeviceCommands => _deviceCommands ?? (_deviceCommands = new List<DeviceCommand>());

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

            var identification = device?.Identification;

            if (identification != null)
            {
                var serialNumber = identification.SerialNumber;

                result = await GetParameter(serialNumber, index, subIndex);
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(EltraDevice device, ushort index, byte subIndex)
        {
            ParameterValue result = null;

            var identification = device?.Identification;

            if (identification != null)
            {
                var serialNumber = identification.SerialNumber;

                result = await GetParameterValue(serialNumber, index, subIndex);
            }

            return result;
        }

        public async Task<List<ParameterValue>> GetParameterHistory(EltraDevice device, string uniqueId, DateTime from, DateTime to)
        {
            List<ParameterValue> result = null;

            var identification = device?.Identification;

            if (identification != null)
            {
                var serialNumber = identification.SerialNumber;

                result = await GetParameterHistory(serialNumber, uniqueId, from, to);
            }

            return result;
        }

        public async Task<List<ParameterUniqueIdValuePair>> GetParameterHistoryPair(EltraDevice device, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            List<ParameterUniqueIdValuePair> result = null;

            var identification = device?.Identification;

            if (identification != null)
            {
                var serialNumber = identification.SerialNumber;

                result = await GetParameterHistoryPair(serialNumber, uniqueId1, uniqueId2, from, to);
            }

            return result;
        }

        #endregion
    }
}

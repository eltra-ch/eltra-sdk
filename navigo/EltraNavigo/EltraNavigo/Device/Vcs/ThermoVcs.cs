using EltraCloudContracts.Contracts.Users;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.Classes;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
using EltraNavigo.Views.Devices.Thermo.History;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EltraNavigo.Device.Vcs
{
    public class ThermoVcs : DeviceVcs
    {
        #region Constructors

        public ThermoVcs(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url, uuid, authData, updateInterval, timeout)
        {
        }

        public ThermoVcs(DeviceAgent agent, EltraCloudContracts.Contracts.Devices.EltraDevice device)
            : base(agent, device)
        {
        }

        #endregion

        #region Methods

        public async Task<ExecuteResult> SetRelayChannelState(UInt16 index, byte state)
        {
            var result = new ExecuteResult();
            var command = await GetVcsCommand("SetChannelState");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                command.SetParameterValue("Index", index);
                command.SetParameterValue("State", state);

                var responseCommand = await Agent.ExecuteCommand(command);

                if (responseCommand != null)
                {
                    responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                    responseCommand.GetParameterValue("Result", ref commandResult);
                }

                result.Result = commandResult;
                result.ErrorCode = lastErrorCode;
            }

            return result;
        }

        public async Task<byte> GetRelayChannelState(UInt16 index)
        {
            byte result = 255;
            var command = await GetVcsCommand("GetChannelState");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                command.SetParameterValue("Index", index);

                var responseCommand = await Agent.ExecuteCommand(command);

                if (responseCommand != null)
                {
                    responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                    responseCommand.GetParameterValue("Result", ref commandResult);
                    responseCommand.GetParameterValue("State", ref result);
                }
            }

            return result;
        }

        public async Task<List<SensorHistoryItem>> GetTemperatureHistory(DateTime from, DateTime to)
        {
            var result = await GetHistory("PARAM_ActualTemperatureAveraged", "PARAM_TemperatureTimestamp", from, to);

            return result;
        }

        public async Task<List<SensorHistoryItem>> GetTemperatureExtHistory(DateTime from, DateTime to)
        {
            var result = await GetHistory("PARAM_ActualExternalTemperatureAveraged", "PARAM_ExternalTemperatureTimestamp", from, to);

            return result;
        }

        private async Task<List<SensorHistoryItem>> GetHistory(string tempUniqueId, string timestampUniqueId, DateTime from, DateTime to)
        {
            var result = new List<SensorHistoryItem>();
            var history = await Agent.GetParameterHistoryPair(Device, tempUniqueId, timestampUniqueId, from, to);

            ParameterValue temperature = null;
            ParameterValue timestamp = null;

            foreach (var item in history)
            {
                if (item.UniqueId == tempUniqueId)
                {
                    temperature = item.Value;
                    timestamp = null;
                }
                else if (item.UniqueId == timestampUniqueId)
                {
                    timestamp = item.Value;
                }

                if (temperature != null && timestamp != null)
                {
                    double temp = 0;
                    DateTime dt = DateTime.MinValue;

                    if (temperature.GetValue(ref temp) && timestamp.GetValue(ref dt))
                    {
                        var pair = new SensorHistoryItem(temp, dt);

                        result.Add(pair);
                    }
                }
            }

            return result;
        }

        public async Task<List<ParameterValue>> GetRelayHistory(DateTime from, DateTime to)
        {
            var history = await Agent.GetParameterHistory(Device, "PARAM_RelayState_1", from, to);

            return history;
        }

        public async Task<List<SensorHistoryItem>> GetHumidityHistory(DateTime from, DateTime to)
        {
            var result = await GetHistory("PARAM_ActualHumidityAveraged", "PARAM_HumidityTimestamp", from, to);

            return result;
        }

        public async Task<List<SensorHistoryItem>> GetHumidityExtHistory(DateTime from, DateTime to)
        {
            var result = await GetHistory("PARAM_ActualExternalHumidityAveraged", "PARAM_ExternalHumidityTimestamp", from, to);

            return result;
        }

        #endregion
    }
}

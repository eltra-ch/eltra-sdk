using EltraCloudContracts.Contracts.Users;
using EltraConnector.Classes;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
using System;
using System.Threading.Tasks;

namespace EltraNavigo.Device.Vcs
{
    public class RelayVcs : DeviceVcs
    {
        #region Constructors

        public RelayVcs(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url, uuid, authData, updateInterval, timeout)
        {
        }

        public RelayVcs(DeviceAgent agent, EltraCloudContracts.Contracts.Devices.EltraDevice device)
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

        #endregion
    }
}

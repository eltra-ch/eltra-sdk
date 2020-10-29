using EltraCommon.Contracts.Devices;
using EltraConnector.Agent;
using EltraConnector.Classes;
using System;
using System.Threading.Tasks;

namespace EltraNavigoRelay.Device.Vcs
{
    public class RelayVcs : VirtualCommandSet
    {
        public RelayVcs(AgentConnector connector, EltraDevice device)
            : base(connector, device)
        {
        }

        #region Methods

        public async Task<ExecuteResult> SetRelayChannelState(UInt16 index, byte state)
        {
            var result = new ExecuteResult();
            var command = await Device.GetCommand("SetChannelState");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                command.SetParameterValue("Index", index);
                command.SetParameterValue("State", state);

                var responseCommand = await command.Execute();

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
            var command = await Device.GetCommand("GetChannelState");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                command.SetParameterValue("Index", index);

                var responseCommand = await command.Execute();

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

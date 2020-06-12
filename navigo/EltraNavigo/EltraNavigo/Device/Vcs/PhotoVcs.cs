using EltraCloudContracts.Contracts.Users;
using EltraConnector.Classes;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
using System;
using System.Threading.Tasks;

namespace EltraNavigo.Device.Vcs
{
    public class PhotoVcs : DeviceVcs
    {
        #region Constructors

        public PhotoVcs(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url, uuid, authData, updateInterval, timeout)
        {
        }

        public PhotoVcs(DeviceAgent agent, EltraCloudContracts.Contracts.Devices.EltraDevice device)
            : base(agent, device)
        {
        }

        #endregion

        #region Methods

        public async Task<ExecuteResult> TakePicture(UInt16 index)
        {
            var result = new ExecuteResult();
            var command = await GetVcsCommand("TakePicture");
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
                }

                result.Result = commandResult;
                result.ErrorCode = (uint)lastErrorCode;
            }

            return result;
        }

        #endregion
    }
}

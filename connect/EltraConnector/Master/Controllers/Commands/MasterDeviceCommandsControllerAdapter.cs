using System;
using System.Threading.Tasks;

using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Contracts.Users;
using EltraConnector.Controllers;

namespace EltraConnector.Master.Controllers.Commands
{
    internal class MasterDeviceCommandsControllerAdapter : DeviceCommandsControllerAdapter
    {
        #region Private fields

        private readonly UserIdentity _userIdentity;
        
        #endregion

        #region Constructors

        public MasterDeviceCommandsControllerAdapter(string url, Channel channel, UserIdentity userIdentity)
            : base(url, channel, userIdentity)
        {
            _userIdentity = userIdentity;
        }

        #endregion

        #region Events handling

        #endregion

        #region Methods

        public override async Task<bool> PushCommand(ExecuteCommand execCommand)
        {
            bool result = false;

            try
            {
                var device = execCommand?.Command?.Device;

                if (device != null)
                {   
                    var wsChannelId = GetActiveChannelName(execCommand.SourceChannelId, device.NodeId);

                    if (!string.IsNullOrEmpty(wsChannelId))
                    {
                        var start = MsgLogger.BeginTimeMeasure();

                        if (ConnectionManager != null && ConnectionManager.IsConnected(wsChannelId))
                        {
                            if (await ConnectionManager.Send(wsChannelId, _userIdentity, execCommand))
                            {
                                result = true;
                            }
                        }

                        MsgLogger.EndTimeMeasure($"{GetType().Name} - PushCommand", start, $"push command='{execCommand.Command.Name}' to device='{device.Family}':0x{device.NodeId}");
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - PushCommand", e);
            }

            return result;
        }

        private string GetActiveChannelName(string channelId, int nodeId)
        {
            string result = string.Empty;

            if (ConnectionManager != null)
            {
                var wsChannelId = channelId + $"_ExecCommander_{nodeId}";

                if (!ConnectionManager.IsConnected(wsChannelId))
                {
                    wsChannelId = channelId + "_Master";

                    if (ConnectionManager.IsConnected(wsChannelId))
                    {
                        result = wsChannelId;
                    }
                    else
                    {
                        wsChannelId = channelId + "_CommandExec";

                        if (ConnectionManager.IsConnected(wsChannelId))
                        {
                            result = wsChannelId;
                        }
                    }
                }
                else
                {
                    result = wsChannelId;
                }
            }

            return result;
        }

        public override async Task<bool> SetCommandStatus(ExecuteCommandStatus status)
        {
            bool result = false;

            if (ConnectionManager != null && status != null)
            {
                var wsChannelId = GetActiveChannelName(status.ChannelId, status.NodeId);

                if (!string.IsNullOrEmpty(wsChannelId))
                {
                    MsgLogger.WriteLine($"{GetType().Name} - SetCommandStatus", 
                        $"set (WS) command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId}");

                    if (await ConnectionManager.Send(wsChannelId, _userIdentity, status))
                    {
                        result = true;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - SetCommandStatus",
                            $"set (WS) command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId} failed");
                    }
                }
            }
            
            return result;
        }

        #endregion
    }
}
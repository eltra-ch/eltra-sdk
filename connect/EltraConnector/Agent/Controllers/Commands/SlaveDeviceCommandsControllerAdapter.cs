using System;
using System.Threading.Tasks;

using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Contracts.Users;
using EltraConnector.Transport.Udp;
using EltraConnector.Controllers;
using EltraCommon.Transport;

namespace EltraConnector.Agent.Controllers.Commands
{
    internal class SlaveDeviceCommandsControllerAdapter : DeviceCommandsControllerAdapter
    {
        #region Private fields

        private readonly IUdpClient _udpClient;

        #endregion

        #region Constructors

        public SlaveDeviceCommandsControllerAdapter(IHttpClient httpClient, IUdpClient udpClient, string url, Channel channel, UserIdentity userIdentity)
            : base(httpClient, url, channel, userIdentity)
        {
            _udpClient = udpClient;

            CommandExecUuid = channel.Id + "_ExecCommander";
        }

        #endregion

        #region Properties

        protected string CommandExecUuid { get; set; }

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
                    var start = MsgLogger.BeginTimeMeasure();

                    if (device.ChannelLocalHost != EltraUdpConnector.LocalHost)
                    {
#if _UDP
                        var udpConnection = ConnectionManager.GetConnection<UdpClientConnection>(CommandExecUuid);

                        if (udpConnection == null)
                        {
                            var udpServerConnection = new UdpClientConnection(_udpClient) { ChannelName = "ExecuteCommander", 
                                                                                  UniqueId = CommandExecUuid, 
                                                                                  Url = device.ChannelLocalHost };
                            
                            await ConnectionManager.Connect(udpServerConnection);
                        }
                        else if(udpConnection.Url != device.ChannelLocalHost)
                        {
                            udpConnection.Url = device.ChannelLocalHost;

                            await udpConnection.Connect();
                        }
#endif
                    }

                    if (ConnectionManager != null && ConnectionManager.IsConnected(CommandExecUuid) && 
                        await ConnectionManager.Send(CommandExecUuid, UserIdentity, execCommand))
                    {
                        result = true;
                    }

                    MsgLogger.EndTimeMeasure($"{GetType().Name} - PushCommand", start, $"push command='{execCommand.Command.Name}' to device='{device.Family}':0x{device.NodeId}");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - PushCommand", e);
            }

            return result;
        }

        public override async Task<bool> SetCommandStatus(ExecuteCommandStatus status)
        {
            bool result = false;

            if (ConnectionManager != null && ConnectionManager.IsConnected(CommandExecUuid))
            {
                MsgLogger.WriteLine($"{GetType().Name} - SetCommandStatus", $"set (WS) command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId}");

                if (await ConnectionManager.Send(CommandExecUuid, UserIdentity, status))
                {
                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - SetCommandStatus",
                        $"set (WS) command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId} failed");
                }
            }
                        
            return result;
        }

#endregion
    }
}
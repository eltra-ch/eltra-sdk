using System;
using System.Threading.Tasks;

using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Contracts.Users;
using EltraConnector.Transport.Udp;
using EltraConnector.Controllers;
using EltraConnector.Transport.Ws.Interfaces;

namespace EltraConnector.Agent.Controllers.Commands
{
    internal class SlaveDeviceCommandsControllerAdapter : DeviceCommandsControllerAdapter
    {
        #region Private fields

        #endregion

        #region Constructors

        public SlaveDeviceCommandsControllerAdapter(string url, Channel channel, UserIdentity userIdentity)
            : base(url, channel, userIdentity)
        {            
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
                        IConnection udpConnection = ConnectionManager.GetConnection<UdpClientConnection>(CommandExecUuid);

                        if (udpConnection == null)
                        {
                            var udpServerConnection = new UdpClientConnection() { ChannelName = "ExecuteCommander", 
                                                                                  UniqueId = CommandExecUuid, 
                                                                                  Url = device.ChannelLocalHost };
                            
                            await ConnectionManager.Connect(udpServerConnection);
                        }
                        else if(udpConnection.Url != device.ChannelLocalHost)
                        {
                            udpConnection.Url = device.ChannelLocalHost;

                            await udpConnection.Connect();
                        }
                    }

                    if (ConnectionManager != null && ConnectionManager.IsConnected(CommandExecUuid))
                    {
                        if (await ConnectionManager.Send(CommandExecUuid, UserIdentity, execCommand))
                        {
                            result = true;
                        }
                    }
                    /*TODO remove 
                    else
                    {
                        var postResult = await Transporter.Post(UserIdentity, Url, "api/command/push", execCommand.ToJson());

                        if (postResult.StatusCode == HttpStatusCode.OK)
                        {
                            result = true;
                        }
                    }*/

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
                MsgLogger.WriteLine($"set (WS) command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId}");

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
            /* TODO remove
            else
            {
                try
                {
                    MsgLogger.WriteLine($"set (REST) command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId}");

                    var postResult = await Transporter.Post(UserIdentity, Url, "api/command/status", JsonConvert.SerializeObject(status));

                    if (postResult.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - SetCommandStatus", e);
                }
            }*/
            
            return result;
        }

        #endregion
    }
}
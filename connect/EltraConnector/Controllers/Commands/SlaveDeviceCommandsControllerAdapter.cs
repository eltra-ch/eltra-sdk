using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

using EltraCommon.Extensions;

using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Contracts.Users;
using EltraConnector.Transport.Udp;
using EltraConnector.Transport.Udp.Contracts;

namespace EltraConnector.Controllers.Commands
{
    internal class SlaveDeviceCommandsControllerAdapter : DeviceCommandsControllerAdapter
    {
        #region Private fields

        private EltraUdpClient _eltraUdpClient;
        private List<UdpRequest> _udpRequestQueue = new List<UdpRequest>();

        #endregion

        #region Constructors

        public SlaveDeviceCommandsControllerAdapter(string url, Channel channel, UserIdentity userIdentity)
            : base(url, channel, userIdentity)
        {            
            CommandExecUuid = channel.Id + "_ExecCommander";
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
                    var start = MsgLogger.BeginTimeMeasure();

                    if (device.ChannelLocalHost != EltraUdpConnector.LocalHost)
                    {
                        if (_eltraUdpClient == null)
                        {
                            _eltraUdpClient = new EltraUdpClient() { Url = device.ChannelLocalHost };

                            _eltraUdpClient.MessageReceived += (s, e) =>
                            {
                                var udpRequest = System.Text.Json.JsonSerializer.Deserialize<UdpRequest>(e.Text);

                                if (udpRequest is UdpRequest)
                                {
                                    udpRequest.Endpoint = e.Endpoint;

                                    if (udpRequest.TypeName == typeof(UdpAckRequest).FullName)
                                    {
                                        //_udpRequestQueue.Add(udpRequest);
                                    }
                                }
                            };
                        }
                    }

                    /*if(_eltraUdpClient!=null)
                    {
                        if(await _eltraUdpClient.Send(_userIdentity, execCommand) > 0)
                        {
                            result = true;
                        }
                    }*/

                    if (_udpRequestQueue.Count == 0)
                    {
                        if (WsConnectionManager != null && WsConnectionManager.IsConnected(CommandExecUuid))
                        {
                            if (await WsConnectionManager.Send(CommandExecUuid, UserIdentity, execCommand))
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            var postResult = await Transporter.Post(UserIdentity, Url, "api/command/push", execCommand.ToJson());

                            if (postResult.StatusCode == HttpStatusCode.OK)
                            {
                                result = true;
                            }
                        }
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

            /*if (_eltraUdpClient != null)
            {
                if (!_master)
                {
                    await _eltraUdpClient.Send(_userIdentity, status);
                }
                else
                {
                    foreach(var udpRequest in _udpRequestQueue)
                    {
                        await UdpServer.Send(udpRequest.Endpoint, _userIdentity, status);
                    }
                }
            }*/

            if (_udpRequestQueue.Count == 0)
            {
                if (WsConnectionManager != null && WsConnectionManager.IsConnected(CommandExecUuid))
                {
                    MsgLogger.WriteLine($"set (WS) command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId}");

                    if (await WsConnectionManager.Send(CommandExecUuid, UserIdentity, status))
                    {
                        result = true;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - SetCommandStatus",
                            $"set (WS) command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId} failed");
                    }
                }
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
                }
            }

            return result;
        }

        #endregion
    }
}
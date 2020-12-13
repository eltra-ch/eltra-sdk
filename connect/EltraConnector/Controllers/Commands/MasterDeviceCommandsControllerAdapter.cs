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
    internal class MasterDeviceCommandsControllerAdapter : DeviceCommandsControllerAdapter
    {
        #region Private fields

        private readonly UserIdentity _userIdentity;
        private List<UdpRequest> _udpRequestQueue = new List<UdpRequest>();
        private EltraUdpServer _udpServer;

        #endregion

        #region Constructors

        public MasterDeviceCommandsControllerAdapter(string url, Channel channel, UserIdentity userIdentity)
            : base(url, channel, userIdentity)
        {
            _userIdentity = userIdentity;
        }

        #endregion

        #region Properties
        public EltraUdpServer UdpServer
        {
            get => _udpServer;
            set
            {
                _udpServer = value;
                OnUdpServerChanged();
            }
        }

        #endregion

        #region Events handling

        private void OnUdpServerChanged()
        {
            if (UdpServer != null)
            {
                UdpServer.MessageReceived += (s, e) =>
                {
                    e.Handled = true;

                    try
                    {
                        var udpRequest = System.Text.Json.JsonSerializer.Deserialize<UdpRequest>(e.Text);

                        if (udpRequest is UdpRequest)
                        {
                            udpRequest.Endpoint = e.Endpoint;

                            //_udpRequestQueue.Add(udpRequest);
                        }
                    }
                    catch (Exception ex)
                    {
                        MsgLogger.Exception($"{GetType().Name} - OnUdpServerChanged", ex);
                    }
                };
            }
        }

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
                    CommandExecUuid = device.ChannelId + $"_ExecCommander_{device.NodeId}";

                    var start = MsgLogger.BeginTimeMeasure();

                    if (_udpRequestQueue.Count == 0)
                    {
                        if (WsConnectionManager != null && WsConnectionManager.IsConnected(CommandExecUuid))
                        {
                            if (await WsConnectionManager.Send(CommandExecUuid, _userIdentity, execCommand))
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            var postResult = await Transporter.Post(_userIdentity, Url, "api/command/push", execCommand.ToJson());

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

            /*if (UdpServer != null)
            {
                foreach(var udpRequest in _udpRequestQueue)
                {
                    await UdpServer.Send(udpRequest.Endpoint, _userIdentity, status);
                }
            }*/

            if (_udpRequestQueue.Count == 0)
            {
                if (WsConnectionManager != null && WsConnectionManager.IsConnected(CommandExecUuid))
                {
                    MsgLogger.WriteLine($"set (WS) command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId}");

                    if (await WsConnectionManager.Send(CommandExecUuid, _userIdentity, status))
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

                        var postResult = await Transporter.Post(_userIdentity, Url, "api/command/status", JsonConvert.SerializeObject(status));

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
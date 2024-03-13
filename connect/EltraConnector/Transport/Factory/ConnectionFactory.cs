using EltraConnector.Transport.Udp;
using EltraConnector.Transport.Ws;
using EltraConnector.Transport.Rest;
using EltraConnector.Transport.Ws.Interfaces;
using System.Collections.Generic;
using EltraCommon.Logger;
using EltraCommon.Transport;

namespace EltraConnector.Transport.Factory
{
    static class ConnectionFactory
    {
#if _UDP
        private static UdpServerConnection _serverConnection;
#endif

        public static List<IConnection> CreateConnections(IHttpClient httpClient, IUdpClient udpClient, IWebSocketClient wsClient, string uniqueId, string channelName, string url)
        {
            var result = new List<IConnection>();

            switch (channelName)
            {
                case "SessionUpdate":
                case "ParameterUpdate":
                    result.Add(new WsConnection(wsClient.Clone(), uniqueId, channelName) { Url = url });
                    result.Add(new RestConnection(httpClient.Clone()) { Url = url, UniqueId = uniqueId, ChannelName = channelName });
                    break;
                case "CommandsExecution":
                case "Master":
#if _UDP
                    _serverConnection = new UdpServerConnection(udpClient.Clone()) { Url = url, ChannelName = channelName, UniqueId = uniqueId };

                    result.Add(_serverConnection);
#endif
                    result.Add(new WsConnection(wsClient.Clone(), uniqueId, channelName) { Url = url });
                    result.Add(new RestConnection(httpClient.Clone()) { Url = url, UniqueId = uniqueId, ChannelName = channelName });
                    break;
                case "ExecuteCommander":
                case "Slave":
#if _UDP
                    if (_serverConnection != null)
                    {
                        result.Add(new UdpServerProxyConnection(_serverConnection) { Url = url, UniqueId = uniqueId, ChannelName = channelName });
                    }
#endif
                    result.Add(new WsConnection(wsClient.Clone(), uniqueId, channelName) { Url = url });
                    result.Add(new RestConnection(httpClient.Clone()) { Url = url, UniqueId = uniqueId, ChannelName = channelName });
                    break;
                default:
                    MsgLogger.WriteError($"ConnectionFactory - CreateConnections", $"cannot create instance, channelName = {channelName}");
                    break;
            }

            return result;
        }
    }
}

using EltraConnector.Transport.Udp;
using EltraConnector.Transport.Ws;
using EltraConnector.Transport.Rest;
using EltraConnector.Transport.Ws.Interfaces;
using System.Collections.Generic;
using EltraCommon.Logger;

namespace EltraConnector.Transport.Factory
{
    static class ConnectionFactory
    {
        public static List<IConnection> CreateConnections(string uniqueId, string channelName, string url)
        {
            var result = new List<IConnection>();

            switch (channelName)
            {
                case "SessionUpdate":
                    result.Add(new WsConnection(uniqueId, channelName) { Url = url });
                    result.Add(new RestConnection() { Url = url, UniqueId = uniqueId, ChannelName = channelName });
                    break;
                case "ParameterUpdate":
                    result.Add(new WsConnection(uniqueId, channelName) { Url = url });
                    result.Add(new RestConnection() { Url = url, UniqueId = uniqueId, ChannelName = channelName });
                    break;
                case "CommandsExecution":
                    result.Add(new UdpServerConnection() { Url = url, ChannelName = channelName, UniqueId = uniqueId });
                    result.Add(new WsConnection(uniqueId, channelName) { Url = url });
                    result.Add(new RestConnection() { Url = url, UniqueId = uniqueId, ChannelName = channelName });
                    break;
                case "ExecuteCommander":
                    result.Add(new WsConnection(uniqueId, channelName) { Url = url });
                    result.Add(new RestConnection() { Url = url, UniqueId = uniqueId, ChannelName = channelName });
                    break;
                default:
                    MsgLogger.WriteError($"ConnectionFactory - CreateConnections", $"cannot create instance, channelName = {channelName}");
                    break;
            }

            return result;
        }
    }
}

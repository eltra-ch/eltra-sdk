using EltraCloud.Services;
using EltraCloud.Channels.Interfaces;
using EltraCloud.Channels.Processors;
using System.Net;
using System.Net.WebSockets;

#pragma warning disable CS1591

namespace EltraCloud.Channels.Factory
{
    public static class ChannelProcessorFactory
    {
        public static IChannelProcessor CreateProcessor(string channelName, IPAddress source, WebSocket webSocket, ISessionService sessionService, Ip2LocationService locationService)
        {
            IChannelProcessor processor = null;

            if (channelName == "SessionUpdate")
            {
                processor = new SessionChannelProcessor(source, webSocket, sessionService, locationService);
            }
            else if (channelName == "CommandsExecution")
            {
                processor = new MasterCommandChannelProcessor(source, webSocket, sessionService);
            }
            else if (channelName == "ExecuteCommander")
            {
                processor = new SlaveCommandChannelProcessor(source, webSocket, sessionService);
            }
            else if (channelName == "ParameterUpdate")
            {
                processor = new ParameterChannelProcessor(source, webSocket, sessionService);
            }

            return processor;
        }

    }
}

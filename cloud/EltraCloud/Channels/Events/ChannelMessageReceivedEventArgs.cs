using EltraCloudContracts.Contracts.Ws;
using System;

#pragma warning disable CS1591

namespace EltraCloud.Channels.Events
{
    public class ChannelMessageReceivedEventArgs : EventArgs
    {
        public WsMessage ChannelMessage { get; set;}
    }
}

using EltraCommon.Contracts.Channels;
using System;

#pragma warning disable 1591

namespace EltraConnector.Events
{
    public class AgentChannelStatusChangedEventArgs : EventArgs
    {
        public string Id { get; set; }
        public ChannelStatus Status { get; set; }
    }
}

using EltraCommon.Contracts.Channels;
using System;

namespace EltraConnector.Events
{
    public class ChannelStatusChangedEventArgs : EventArgs
    {
        public string Id { get; set; }
        public ChannelStatus Status { get; set; }
    }
}

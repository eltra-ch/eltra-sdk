using EltraCommon.Contracts.Channels;
using System;

namespace EltraConnector.Events
{
    public class SessionStatusChangedEventArgs : EventArgs
    {
        public string Uuid { get; set; }
        public ChannelStatus Status { get; set; }
    }
}

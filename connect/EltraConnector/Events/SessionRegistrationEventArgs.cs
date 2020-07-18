using System;
using EltraCommon.Contracts.Channels;

namespace EltraConnector.Events
{
    public class ChannelRegistrationEventArgs : EventArgs
    {
        public Channel Channel { get; set; }

        public bool Success { get; set; }

        public Exception Exception { get; set; }
    }
}

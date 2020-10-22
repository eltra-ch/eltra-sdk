using System;

namespace EltraConnector.Channels.Events
{
    class WsChannelStatusEventArgs : EventArgs
    {
        public WsChannelStatus Status { get; set; }
    }
}

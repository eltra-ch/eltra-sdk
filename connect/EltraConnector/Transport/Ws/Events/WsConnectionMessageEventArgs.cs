using System;

namespace EltraConnector.Transport.Ws.Events
{
    enum WsMessageType
    {
        Unknown,
        Text,
        Json,
        Data,
        WsMessage
    }

    class WsConnectionMessageEventArgs : EventArgs
    {
        public string Source { get; set; }
        public string Message { get; set; }

        public WsMessageType Type { get; set; }
    }
}

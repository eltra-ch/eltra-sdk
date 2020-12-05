using System;

namespace EltraConnector.Transport.Ws.Events
{
    /// <summary>
    /// WsMessageType
    /// </summary>
    public enum WsMessageType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Text
        /// </summary>
        Text,
        /// <summary>
        /// Json
        /// </summary>
        Json,
        /// <summary>
        /// Data
        /// </summary>
        Data,
        /// <summary>
        /// WsMessage
        /// </summary>
        WsMessage
    }

    /// <summary>
    /// WsConnectionMessageEventArgs
    /// </summary>
    public class WsConnectionMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Source
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public WsMessageType Type { get; set; }
    }
}

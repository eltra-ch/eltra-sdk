using System;

namespace EltraConnector.Transport.Events
{
    /// <summary>
    /// WsMessageType
    /// </summary>
    public enum MessageType
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
    /// ConnectionMessageEventArgs
    /// </summary>
    public class ConnectionMessageEventArgs : EventArgs
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
        public MessageType Type { get; set; }

        /// <summary>
        /// Is control message ACK or KEEPALIVE 
        /// </summary>
        /// <returns></returns>
        public bool IsControlMessage()
        {
            bool result = false;
            var msg = Message.Trim(new char[] { '\"' });

            if (msg == "ACK" || msg == "KEEPALIVE")
            {
                result = true;
            }

            return result;
        }
    }
}

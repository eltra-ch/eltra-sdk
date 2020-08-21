using System;
using System.Net.Sockets;

namespace EltraConnector.Transport.Events
{
    /// <summary>
    /// SocketErrorChangedEventAgs
    /// </summary>
    public class SocketErrorChangedEventAgs : EventArgs
    {
        /// <summary>
        /// SocketError
        /// </summary>
        public SocketError SocketError { get; set; }
    }
}

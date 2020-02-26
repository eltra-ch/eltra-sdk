using System;
using System.Net.Sockets;

namespace EltraConnector.Transport.Events
{
    public class SocketErrorChangedEventAgs : EventArgs
    {
        public SocketError SocketError { get; set; }
    }
}

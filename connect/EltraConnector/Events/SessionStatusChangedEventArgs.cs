using EltraCommon.Contracts.Sessions;
using System;

namespace EltraConnector.Events
{
    public class SessionStatusChangedEventArgs : EventArgs
    {
        public string Uuid { get; set; }
        public SessionStatus Status { get; set; }
    }
}

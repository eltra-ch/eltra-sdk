using System;

using EltraCommon.Contracts.Sessions;

namespace EltraConnector.Events
{
    public class SessionRegistrationEventArgs : EventArgs
    {
        public Session Session { get; set; }

        public bool Success { get; set; }

        public Exception Exception { get; set; }
    }
}

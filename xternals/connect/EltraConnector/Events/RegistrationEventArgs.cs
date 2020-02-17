using System;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Sessions;

namespace EltraConnector.Events
{
    public class RegistrationEventArgs : EventArgs
    {
        public Session Session { get; set; }
        public EltraDevice Device { get; set; }

        public RegistrationState State { get; set; }

        public Exception Exception { get; set; }
    }
}

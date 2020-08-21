using System;
using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Devices;

#pragma warning disable 1591

namespace EltraConnector.Events
{
    public class RegistrationEventArgs : EventArgs
    {
        private Exception _exception;

        public Channel Channel { get; set; }
        public EltraDevice Device { get; set; }

        public RegistrationState State { get; set; }

        public Exception Exception 
        { 
            get => _exception ?? (_exception = new NotImplementedException()); 
            set => _exception = value; 
        }

        public string Reason { get; set; }
    }
}

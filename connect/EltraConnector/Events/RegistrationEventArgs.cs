using System;
using EltraCommon.Contracts.Node;
using EltraCommon.Contracts.Channels;

namespace EltraConnector.Events
{
    public class RegistrationEventArgs : EventArgs
    {
        private Exception _exception;

        public Channel Session { get; set; }
        public EltraDeviceNode Device { get; set; }

        public RegistrationState State { get; set; }

        public Exception Exception 
        { 
            get => _exception ?? (_exception = new NotImplementedException()); 
            set => _exception = value; 
        }

        public string Reason { get; set; }
    }
}

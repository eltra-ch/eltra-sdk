using System;

namespace EltraNavigo.Views.Obd.Outputs.Events
{
    public class ObdEventAddedEventArgs : EventArgs
    {
        public ObdEntry Entry { get; set; }
    }
}

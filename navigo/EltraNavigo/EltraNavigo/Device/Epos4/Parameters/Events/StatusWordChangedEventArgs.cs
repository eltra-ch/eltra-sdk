using System;

namespace EltraNavigo.Device.Epos4.Parameters.Events
{
    public class StatusWordChangedEventArgs : EventArgs
    {
        public ushort StatusWordValue { get; set; }
    }
}

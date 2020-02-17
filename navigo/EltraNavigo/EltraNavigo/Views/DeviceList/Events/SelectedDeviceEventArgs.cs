using System;

namespace EltraNavigo.Views.DeviceList.Events
{
    public class SelectedDeviceEventArgs : EventArgs
    {
        public DeviceViewModel Device { get; set; }
    }
}

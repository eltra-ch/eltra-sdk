using EltraConnector.Master.Device;
using System;

namespace EltraMaster.DeviceManager.Events
{
    public class DeviceCommunicationEventArgs : EventArgs
    {
        public MasterDevice Device { get; set; }

        public uint LastErrorCode { get; set; }
    }
}

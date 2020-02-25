using System;
using EposMaster.DeviceManager.Device;

namespace EposMaster.DeviceManager.Events
{
    class DeviceVersionEventArgs : EventArgs
    {
        public EposDevice Device { get; set; }

        public EposDeviceVersion Version { get; set; }

        public uint LastErrorCode { get; set; }

        public Exception Exception { get; set; }
    }
}

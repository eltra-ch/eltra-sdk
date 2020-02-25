using System;
using EltraCloudContracts.Contracts.Devices;

namespace EposMaster.DeviceManager.Identification.Events
{
    public class DeviceIdentificationEventArgs : EventArgs
    {
        public EltraDevice Device { get; set; }

        public ulong SerialNumber { get; set; }

        public uint LastErrorCode { get; set; }

        public DeviceIdentificationState State { get; set; }

        public Exception Exception { get; set; }
    }
}

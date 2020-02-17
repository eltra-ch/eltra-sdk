using System;
using ThermoMaster.DeviceManager.Device;

namespace ThermoMaster.DeviceManager.Events
{
    class DeviceCommunicationEventArgs : EventArgs
    {
        public ThermoDeviceBase Device { get; set; }

        public uint LastErrorCode { get; set; }
    }
}

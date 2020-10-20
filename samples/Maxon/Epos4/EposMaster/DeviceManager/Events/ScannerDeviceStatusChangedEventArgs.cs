using System;
using EposMaster.DeviceManager.Device;
using EposMaster.DeviceManager.Scanner;

namespace EposMaster.DeviceManager.Events
{
    class ScannerDeviceStatusChangedEventArgs : EventArgs
    {
        public MotionControllerDevice Device { get; set; }        
    }
}

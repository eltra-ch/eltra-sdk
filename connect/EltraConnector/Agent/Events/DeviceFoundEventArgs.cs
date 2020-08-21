using EltraCommon.Contracts.Devices;
using System;

namespace EltraConnector.Agent.Events
{
    /// <summary>
    /// DeviceDetectedEventArgs - device detected event args
    /// </summary>
    public class DeviceDetectedEventArgs : EventArgs
    {
        /// <summary>
        /// {EltraDeviceNode} device instance
        /// </summary>
        public EltraDevice Device { get; set; }
    }
}

using EltraConnector.Master.Device;
using System;

namespace EltraMaster.DeviceManager.Events
{
    /// <summary>
    /// {DeviceCommunicationEventArgs} - device communication event arguments
    /// </summary>
    public class DeviceCommunicationEventArgs : EventArgs
    {
        /// <summary>
        /// {MasterDevice} Device instance
        /// </summary>
        public MasterDevice Device { get; set; }

        /// <summary>
        /// LastErrorCode - device error code
        /// </summary>
        public uint LastErrorCode { get; set; }
    }
}

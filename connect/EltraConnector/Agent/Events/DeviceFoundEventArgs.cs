using EltraCommon.Contracts.Node;
using System;

namespace EltraConnector.Agent.Events
{
    public class DeviceFoundEventArgs : EventArgs
    {
        public EltraDeviceNode Device { get; set; }
    }
}

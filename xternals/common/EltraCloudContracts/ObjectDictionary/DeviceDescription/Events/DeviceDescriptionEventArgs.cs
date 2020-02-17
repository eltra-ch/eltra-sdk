using System;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription.Events
{
    public class DeviceDescriptionEventArgs : EventArgs
    {
        public DeviceDescriptionFile DeviceDescription { get; set; }

        public DeviceDescriptionState State { get; set; }

        public Exception Exception { get; set; }
    }
}

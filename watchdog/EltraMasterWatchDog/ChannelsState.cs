using System;

namespace EltraMasterWatchDog
{
    class ChannelsState
    {
        public ChannelsState()
        {
            IsDeviceOnline = false;
            LastDeviceOnline = DateTime.MinValue;
        }

        public bool IsDeviceOnline { get; set; }
        public DateTime LastDeviceOnline { get; set; }
    }
}

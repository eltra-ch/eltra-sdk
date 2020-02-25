using EltraMaster.DeviceManager.Events;

namespace EposMaster.DeviceManager.Events
{
    public class EposCommunicationEventArgs : DeviceCommunicationEventArgs
    {
        public EposCommunicationStatus Status { get; set; }
    }
}

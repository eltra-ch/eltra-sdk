using EltraConnector.UserAgent.Vcs;

namespace EltraMaster.Device
{
    public class MasterVcs : DeviceVcs
    {
        public MasterVcs(MasterDevice device, uint updateInterval, uint timeout)
            : base(device.CloudAgent.Url, device.SessionUuid, device.CloudAgent.AuthData, updateInterval, timeout)
        {
            Device = device;
        }
    }
}

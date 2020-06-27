using EltraCloudContracts.Contracts.Devices;
using EltraConnector.SyncAgent;
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

        public MasterVcs(SyncCloudAgent masterAgent, EltraDevice device, uint updateInterval, uint timeout)
            : base(masterAgent.Url, device.SessionUuid, masterAgent.AuthData, updateInterval, timeout)
        {
            Device = device;
        }
    }
}

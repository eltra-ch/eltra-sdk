using EltraCommon.Contracts.Sessions;
using EltraConnector.SyncAgent;
using EltraConnector.UserAgent.Vcs;

namespace EltraConnector.Master.Device
{
    public class MasterVcs : DeviceVcs
    {
        public MasterVcs(MasterDevice device, uint updateInterval, uint timeout)
            : base(device.CloudAgent.Url, device.CloudAgent.SessionUuid, device.CloudAgent.AuthData, updateInterval, timeout)
        {
            Device = device;
        }

        public MasterVcs(SyncCloudAgent masterAgent, SessionDevice device, uint updateInterval, uint timeout)
            : base(masterAgent.Url, device.SessionUuid, masterAgent.AuthData, updateInterval, timeout)
        {
            Device = device.Device;
        }
    }
}

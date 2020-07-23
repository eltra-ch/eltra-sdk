using EltraCommon.Contracts.Node;
using EltraConnector.SyncAgent;
using EltraConnector.UserAgent.Vcs;

namespace EltraConnector.Master.Device
{
    public class MasterVcs : DeviceVcs
    {
        public MasterVcs(MasterDevice device, uint updateInterval, uint timeout)
            : base(device.CloudAgent.Url, device.CloudAgent.ChannelId, device.CloudAgent.AuthData, updateInterval, timeout)
        {
            Device = device;
        }

        public MasterVcs(SyncCloudAgent masterAgent, EltraDeviceNode device, uint updateInterval, uint timeout)
            : base(masterAgent.Url, device.ChannelId, masterAgent.AuthData, updateInterval, timeout)
        {
            Device = device;
        }

        public MasterVcs(MasterDevice device)
            : base(device.CloudAgent.Url, device.ChannelId, device.CloudAgent.AuthData, device.CloudAgent.UpdateInterval, device.CloudAgent.Timeout)
        {
            Device = device;
        }
    }
}

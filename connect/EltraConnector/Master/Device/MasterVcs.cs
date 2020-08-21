using EltraCommon.Contracts.Devices;
using EltraConnector.SyncAgent;
using EltraConnector.UserAgent.Vcs;

namespace EltraConnector.Master.Device
{
    /// <summary>
    /// MasterVcs
    /// </summary>
    public class MasterVcs : DeviceVcs
    {
        /// <summary>
        /// MasterVcs
        /// </summary>
        /// <param name="device"></param>
        /// <param name="updateInterval"></param>
        /// <param name="timeout"></param>
        public MasterVcs(MasterDevice device, uint updateInterval, uint timeout)
            : base(device.CloudAgent.Url, device.CloudAgent.ChannelId, device.CloudAgent.Identity, updateInterval, timeout)
        {
            Device = device;
        }

        /// <summary>
        /// MasterVcs
        /// </summary>
        /// <param name="masterAgent"></param>
        /// <param name="device"></param>
        /// <param name="updateInterval"></param>
        /// <param name="timeout"></param>
        public MasterVcs(SyncCloudAgent masterAgent, EltraDevice device, uint updateInterval, uint timeout)
            : base(masterAgent.Url, device.ChannelId, masterAgent.Identity, updateInterval, timeout)
        {
            Device = device;
        }

        /// <summary>
        /// MasterVcs
        /// </summary>
        /// <param name="device"></param>
        public MasterVcs(MasterDevice device)
            : base(device.CloudAgent, device, device.CloudAgent.UpdateInterval, device.CloudAgent.Timeout)
        {
            Device = device;
        }
    }
}

using EltraCommon.Contracts.Devices;
using EltraConnector.Agent;
using EltraXamCommon.Device.Factory;

namespace EltraNavigoRelay.Device.Vcs.Factory
{
    public class RelayVcsFactory : IDeviceVcsFactory
    {
        public VirtualCommandSet CreateVcs(AgentConnector agent, EltraDevice device)
        {
            return new RelayVcs(agent, device);
        }
    }
}

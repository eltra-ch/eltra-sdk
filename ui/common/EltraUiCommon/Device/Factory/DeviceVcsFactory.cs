using EltraCommon.Contracts.Devices;
using EltraConnector.Agent;

namespace EltraUiCommon.Device.Factory
{
    public interface IDeviceVcsFactory
    {
        VirtualCommandSet CreateVcs(AgentConnector agent, EltraDevice device);
    }
}

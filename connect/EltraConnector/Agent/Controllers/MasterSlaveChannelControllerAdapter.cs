using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Transport;
using EltraConnector.SyncAgent;
using EltraConnector.Transport.Udp;
using System.Threading.Tasks;

namespace EltraConnector.Agent.Controllers
{
    class MasterSlaveChannelControllerAdapter : SlaveChannelControllerAdapter
    {
        public MasterSlaveChannelControllerAdapter(IHttpClient httpClient, IUdpClient udpClient, SyncCloudAgent masterAgent, EltraDevice deviceNode, uint updateInterval, uint timeout)
            : base(httpClient, udpClient, masterAgent.Url, deviceNode.ChannelId, masterAgent.Identity, updateInterval, timeout)
        {            
        }

        internal override Task<bool> SetParameterValue(EltraDevice device, ushort index, byte subIndex, ParameterValue parameterValue)
        {
            return DeviceAdapter.SetParameterValue(device.NodeId, index, subIndex, parameterValue);
        }
    }
}

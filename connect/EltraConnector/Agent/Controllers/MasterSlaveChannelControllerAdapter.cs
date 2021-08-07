using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.SyncAgent;
using System.Threading.Tasks;

namespace EltraConnector.Agent.Controllers
{
    class MasterSlaveChannelControllerAdapter : SlaveChannelControllerAdapter
    {
        public MasterSlaveChannelControllerAdapter(SyncCloudAgent masterAgent, EltraDevice deviceNode, uint updateInterval, uint timeout)
            : base(masterAgent.Url, deviceNode.ChannelId, masterAgent.Identity, updateInterval, timeout)
        {            
        }

        internal override Task<bool> SetParameterValue(EltraDevice device, ushort index, byte subIndex, ParameterValue parameterValue)
        {
            return DeviceAdapter.SetParameterValue(device.NodeId, index, subIndex, parameterValue);
        }
    }
}

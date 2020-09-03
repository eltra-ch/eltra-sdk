using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Users;
using EltraConnector.Agent;
using System.Threading.Tasks;

namespace TestEltraConnector
{
    class AgentConnectorTestData
    {
        private readonly AgentConnector _connector;
        private readonly UserIdentity _identity;

        public AgentConnectorTestData(AgentConnector connector, UserIdentity identity)
        {
            _connector = connector;
            _identity = identity;
        }

        public async Task<EltraDevice> GetDevice(int nodeId)
        {
            EltraDevice result = null;
            bool signInResult = await _connector.SignIn(_identity, true);

            if (signInResult)
            {
                var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
                
                var connectResult = await _connector.Connect(deviceIdentity);

                if (connectResult)
                {
                    var channels = await _connector.GetChannels();

                    foreach (var channel in channels)
                    {
                        foreach (var device in channel.Devices)
                        {
                            if (device.NodeId == nodeId)
                            {
                                result = device;
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}

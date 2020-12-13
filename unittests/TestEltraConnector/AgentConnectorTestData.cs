using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Users;
using EltraConnector.Agent;
using System.Threading.Tasks;

namespace TestEltraConnector
{
    public class AgentConnectorTestData
    {
        private readonly AgentConnector _connector;
        private readonly UserIdentity _identity;

        public AgentConnectorTestData(AgentConnector connector, UserIdentity identity)
        {
            _connector = connector;
            _identity = identity;
        }

        public async Task<EltraDevice> GetDevice(int nodeId, string deviceLogin, string devicePassword)
        {
            EltraDevice result = null;

            if(string.IsNullOrEmpty(Settings.Default.LoginName))
            {
                Settings.Default.LoginName = _identity.Login;
                Settings.Default.Password = _identity.Password;
                
                Settings.Default.Save();
            }
            else
            {
                _identity.Login = Settings.Default.LoginName;
                _identity.Password = Settings.Default.Password;
            }

            bool signInResult = await _connector.SignIn(_identity, true);

            if (signInResult)
            {
                var deviceIdentity = new UserIdentity() { Login = deviceLogin, Password = devicePassword };
                
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

using EltraCommon.Contracts.Users;
using System.Threading.Tasks;
using Xunit;

namespace TestEltraConnector
{
    [CollectionDefinition(nameof(BindingTest), DisableParallelization = true)]
    public class BindingTest : AgentTestBase
    {        
        [Fact]
        public async Task Channels_ChannelCanBeBinded()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = _aliasDeviceLogin, Password = _aliasDevicePassword };
            var connectResult = await _connector.Connect();

            //Act
            var bindResult = await _connector.BindChannels(deviceIdentity);

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(bindResult, "Bind channels failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_ChannelCanBeBindedAndUnbound()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(Identity, true);
            var deviceIdentity = new UserIdentity() { Login = _aliasDeviceLogin, Password = _aliasDevicePassword };
            var connectResult = await _connector.Connect();
            var bindResult = await _connector.BindChannels(deviceIdentity);

            Assert.True(bindResult, "Bind channels failed.");

            var channels = await _connector.GetChannels();

            Assert.True(channels.Count > 0, "Get channels failed (1).");

            foreach (var channel in channels)
            {
                var unbindResult = await _connector.UnbindChannel(channel);

                Assert.True(unbindResult, "Un-bind channels failed.");
            }

            channels = await _connector.GetChannels();

            Assert.True(channels.Count == 0, $"Binded channels remains count = {channels.Count}.");

            //Assert            
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");

            await _connector.SignOut();
        }
    }
}

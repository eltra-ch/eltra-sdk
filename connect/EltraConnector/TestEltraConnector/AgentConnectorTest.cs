using EltraConnector.Agent;
using EltraCommon.Contracts.Users;
using Xunit;
using System.Threading.Tasks;
using System;

namespace TestEltraConnector
{
    public class AgentConnectorTest : IDisposable
    {
        private AgentConnector _connector;
        private UserIdentity _identity;

        public AgentConnectorTest()
        {
            string host = "https://eltra.ch";
            //string host = "http://localhost:5001";

            _connector = new AgentConnector() { Host = host };

            _identity = new UserIdentity()
            {
                Login = Guid.NewGuid().ToString(),
                Password = "123456",
                Name = "Unit test user",
                Role = "developer"
            };
        }

        [Fact]
        public async Task Authentication_SignInWithAccountCreationShouldSucceed()
        {
            //Arrange
            
            //Act
            var result = await _connector.SignIn(_identity, true);

            //Assert
            Assert.True(result);            
        }

        [Fact]
        public async Task Authentication_SignInWithoutAccountCreationShouldFail()
        {
            //Arrange

            //Act
            var result = await _connector.SignIn(_identity);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Authentication_ConnectToTestMasterShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);

            //Act
            var result = await _connector.Connect();

            //Assert
            Assert.True(result, "Connect failed.");
            Assert.True(signInResult, "Sign-in failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Authentication_ConnectAndBindToTestMasterShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            
            var deviceIdentity = new UserIdentity() { Login = "test.master@eltra.ch", Password = "1234" };

            //Act
            var result = await _connector.Connect(deviceIdentity);

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(result, "Connect failed.");
            
            await _connector.SignOut();
        }

        [Fact]
        public async Task Authentication_ConnectAndBindToTestMasterUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);

            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };

            //Act
            var result = await _connector.Connect(deviceIdentity);

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(result, "Connect failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_GetChannelsUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);

            //Act
            var channels = await _connector.GetChannels();

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_GetChannelLocationCountryCodeUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();

            //Act
            string countryCode = string.Empty;
            foreach(var channel in channels)
            {
                countryCode = channel.Location.CountryCode;
            }

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.False(string.IsNullOrEmpty(countryCode), "Get country code failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Channels_GetChannelOwnerUserNameUsingAliasShouldSucceed()
        {
            //Arrange
            bool signInResult = await _connector.SignIn(_identity, true);
            var deviceIdentity = new UserIdentity() { Login = "test@eltra.ch", Password = "1234" };
            var connectResult = await _connector.Connect(deviceIdentity);
            var channels = await _connector.GetChannels();

            //Act
            string channelOwnerUserName = string.Empty;
            foreach (var channel in channels)
            {
                channelOwnerUserName = channel.UserName;
            }

            //Assert
            Assert.True(signInResult, "Sign-in failed.");
            Assert.True(connectResult, "Connect failed.");
            Assert.True(channels.Count > 0, "Get channels failed.");
            Assert.False(string.IsNullOrEmpty(channelOwnerUserName), "Get country code failed.");

            await _connector.SignOut();
        }

        [Fact]
        public async Task Authentication_SignOutShouldSucceed()
        {
            //Arrange
            await _connector.SignIn(_identity, true);

            //Act
            var result = await _connector.SignOut();

            //Assert
            Assert.True(result);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool release)
        {
            //Cleanup
            var t = Task.Run(async ()=>
            {
                await _connector.SignOff();
            });

            t.Wait();

            _connector.Dispose();
        }
    }
}

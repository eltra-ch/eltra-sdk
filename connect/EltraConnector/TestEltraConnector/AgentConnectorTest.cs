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
            _connector = new AgentConnector() { Host = "https://eltra.ch" };

            _identity = new UserIdentity()
            {
                Login = Guid.NewGuid().ToString(),
                Password = "123456",
                Name = "Unit test user",
                Role = "developer"
            };
        }

        [Fact]
        public async Task Authentication_SignInShouldSucceed()
        {
            //Arrange
            
            //Act
            var result = await _connector.SignIn(_identity, true);

            //Assert
            Assert.True(result);            
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

using EltraConnector.Agent;
using EltraCommon.Contracts.Users;
using Xunit;
using System.Threading.Tasks;
using System;

namespace TestEltraConnector
{
    public class AgentConnectorTest
    {
        [Fact]
        public async Task Test1()
        {
            var identity = new UserIdentity() { Login = Guid.NewGuid().ToString(), Password = "123456", Name = "Unit test user", Role = "developer" };

            //Arrange
            var connector = new AgentConnector() { Host = "https://eltra.ch" };

            //Act
            var signInResult = await connector.SignIn(identity, true);

            //connector.SignOff();

            //Assert
        }
    }
}

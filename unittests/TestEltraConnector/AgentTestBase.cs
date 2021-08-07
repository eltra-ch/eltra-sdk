using EltraCommon.Contracts.Users;
using EltraConnector.Agent;
using System;
using System.Threading.Tasks;

namespace TestEltraConnector
{
    public class AgentTestBase : IDisposable
    {
        protected string _host = "https://eltra.ch";
        //protected string _host = "http://localhost:5001";

        protected AgentConnector _connector;
        private static UserIdentity _identity;
        private AgentConnectorTestData _testData;
        protected string _aliasDeviceLogin = "test4@eltra.ch";
        protected string _aliasDevicePassword = "1234";
        protected string masterDeviceLogin = "test.master4@eltra.ch";

        public AgentTestBase()
        {
            _connector = new AgentConnector() { Host = _host };
        }

        protected AgentConnectorTestData TestData
        {
            get => _testData ?? (_testData = new AgentConnectorTestData(_connector, Identity));
        }

        protected static UserIdentity Identity
        {
            get => _identity ?? (_identity = CreateUserIdentity());
        }

        protected static UserIdentity CreateUserIdentity()
        {
            return new UserIdentity()
            {
                Login = Guid.NewGuid().ToString() + "@eltra.ch",
                Password = "123456",
                Name = "Unit test user",
                Role = "developer"
            };
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool release)
        {
            //Cleanup
            var t = Task.Run(async () =>
            {
                await _connector.SignOff();
            });

            t.Wait();

            _connector.Dispose();
        }
    }
}

using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;
using EltraCloudContracts.Contracts.Users;

namespace ThermoMaster
{
    class Authentication
    {
        private readonly SyncCloudAgent _agent;
        private UserAuthData _authData;
        private string _token;

        public Authentication(SyncCloudAgent agent)
        {
            _agent = agent;
        }

        public async Task<bool> SignIn()
        {
            _token = await _agent.SignIn(_authData);

            return !string.IsNullOrEmpty(_token);
        }

        public async Task<bool> SignOut()
        {
            return await _agent.SignOut(_token);
        }

        public async Task<bool> IsValid()
        {
            return await _agent.IsAuthValid(_authData);
        }

        public async Task<bool> Login(UserAuthData authData)
        {
            bool result = true;

            _authData = authData;

            if (!await _agent.LoginExists(authData.Login))
            {
                if (await _agent.Register(authData))
                {
                    MsgLogger.Print($"New login {authData.Login} registered successfully");
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Login", $"Auth data registration failed!");
                    result = false;
                }
            }

            return result;
        }
    }
}

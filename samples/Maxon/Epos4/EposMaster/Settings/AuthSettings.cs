using EltraCommon.Contracts.Users;
using Microsoft.Extensions.Configuration;

namespace EposMaster.Settings
{
    public class AuthSettings
    {
        private readonly IConfiguration _config;

        public AuthSettings(IConfiguration config)
        {
            _config = config;
        }

        public UserIdentity AuthData => new UserIdentity { Login = Login, Name = Name, Password = Password, Role = "developer"};

        public string Login
        {
            get
            {
                string result = _config["Auth:Login"];

                return result;
            }
            set
            {
                _config["Auth:Login"] = value;
            }
        }

        public string Name
        {
            get
            {
                string result = _config["Auth:Name"];

                return result;
            }
            set
            {
                _config["Auth:Name"] = value;
            }
        }

        public string Password
        {
            get
            {
                string result = _config["Auth:Password"];

                return result;
            }
            set
            {
                _config["Auth:Password"] = value;
            }
        }

        public string Role
        {
            get
            {
                string result = _config["Auth:Role"];

                return result;
            }
            set
            {
                _config["Auth:Role"] = value;
            }
        }
    }
}

using EltraCloudContracts.Contracts.Users;
using Microsoft.Extensions.Configuration;

namespace PhotoMaster.Settings
{
    public class AuthSettings
    {
        private readonly IConfiguration _config;

        public AuthSettings(IConfiguration config)
        {
            _config = config;
        }

        public UserAuthData AuthData => new UserAuthData { Login = Login, Name = Name, Password = Password};

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
    }
}

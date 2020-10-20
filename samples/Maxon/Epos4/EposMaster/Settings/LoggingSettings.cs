using EltraCommon.Logger;
using Microsoft.Extensions.Configuration;

namespace EposMaster.Settings
{
    public class LoggingSettings
    {
        private readonly IConfiguration _config;

        public LoggingSettings(IConfiguration config)
        {
            _config = config;
        }

        public string LogLevels
        {
            get
            {   
                string result = _config["Logging:LogLevels"];

                return result;
            }
            set
            {
                _config["Logging:LogLevels"] = value;

                MsgLogger.LogLevels = value;
            }
        }
    }
}

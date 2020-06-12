using EltraCommon.Logger;
using Microsoft.Extensions.Configuration;

namespace PhotoMaster.Settings
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
                string result = _config["Logging:Levels"];

                return result;
            }
            set
            {
                _config["Logging:Levels"] = value;

                MsgLogger.LogLevels = value;
            }
        }

        public string LogOutputs
        {
            get
            {
                string result = _config["Logging:Outputs"];

                return result;
            }
            set
            {
                _config["Logging:Outputs"] = value;

                MsgLogger.LogOutputs = value;
            }
        }
    }
}

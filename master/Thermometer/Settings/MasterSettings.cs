using System.IO;
using EltraCommon.Logger;
using Microsoft.Extensions.Configuration;

namespace ThermoMaster.Settings
{
    public class MasterSettings
    {
        #region Private fields

        private static IConfiguration _configuration;
        private static DeviceSettings _deviceSettings;
        private static LoggingSettings _loggingSettings;
        private static AuthSettings _authSettings;

        #endregion

        #region Constructors

        public MasterSettings()
        {
            SetupLogger();
        }

        #endregion

        #region Properties

        public string Host
        {
            get
            {
                return Configuration["Host"];
            }
            set
            {
                Configuration["Host"] = value;
            }
        }

        public uint Timeout
        {
            get
            {
                uint result = uint.MaxValue;
                string timeoutText = Configuration["Timeout"];

                if (!string.IsNullOrEmpty(timeoutText))
                {
                    if (uint.TryParse(timeoutText, out uint timeout))
                    {
                        result = timeout;
                    }
                }

                return result;                
            }
        }

        public uint UpdateInterval
        {
            get
            {
                const uint defaultUpdateInterval = 60;
                uint result = defaultUpdateInterval;
                string updateIntervalText = Configuration["UpdateInterval"];

                if (!string.IsNullOrEmpty(updateIntervalText))
                {
                    if (uint.TryParse(updateIntervalText, out uint val))
                    {
                        result = val;
                    }
                }

                return result;
            }
        }

        private IConfiguration Configuration
        {
            get => _configuration ?? (_configuration = new ConfigurationBuilder()
                                                            .SetBasePath(Directory.GetCurrentDirectory())
                                                            .AddJsonFile("appsettings.json", true, true)
                                                            .Build());
        }

        public DeviceSettings Device 
        { 
            get => _deviceSettings ?? (_deviceSettings = new DeviceSettings(Configuration));
        }

        public LoggingSettings Logging 
        { 
            get => _loggingSettings ?? (_loggingSettings = new LoggingSettings(Configuration));
        }

        public AuthSettings Auth 
        { 
            get => _authSettings ?? (_authSettings = new AuthSettings(Configuration));
        }

        #endregion

        #region Private fields

        private void SetupLogger()
        {
            var logLevels = Logging.LogLevels;

            if (!string.IsNullOrEmpty(logLevels))
            {
                MsgLogger.LogLevels = logLevels;
            }

            var logOutputs = Logging.LogOutputs;

            if (!string.IsNullOrEmpty(logOutputs))
            {
                MsgLogger.LogOutputs = logOutputs;
            }
        }

        #endregion
    }
}

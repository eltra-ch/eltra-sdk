using System.Collections.Generic;
using System.IO;
using System.Linq;
using EltraCommon.Logger;
using Microsoft.Extensions.Configuration;

namespace EposMaster.Settings
{
    public class MasterSettings
    {
        #region Private fields

        private static IConfiguration _configuration;
        private static ScanningSettings _scanningSettings;
        private static LoggingSettings _loggingSettings;
        private static AuthSettings _authSettings;

        #endregion

        #region Constructors

        public MasterSettings()
        {
            InitLogLevels();
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

        private IConfiguration Configuration
        {
            get => _configuration ?? (_configuration = new ConfigurationBuilder()
                                                            .SetBasePath(Directory.GetCurrentDirectory())
                                                            .AddJsonFile("appsettings.json", true, true)
                                                            .Build());
        }

        public ScanningSettings Scanning 
        { 
            get => _scanningSettings ?? (_scanningSettings = new ScanningSettings(Configuration));
        }

        public LoggingSettings Logging 
        { 
            get => _loggingSettings ?? (_loggingSettings = new LoggingSettings(Configuration));
        }

        public AuthSettings Identity 
        { 
            get => _authSettings ?? (_authSettings = new AuthSettings(Configuration));
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

        #endregion

        #region Private fields

        private void InitLogLevels()
        {
            var logLevels = Logging.LogLevels;

            if (!string.IsNullOrEmpty(logLevels))
            {
                MsgLogger.LogLevels = logLevels;
            }
        }

        #endregion
    }
}

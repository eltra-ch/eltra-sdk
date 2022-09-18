using System.IO;
using EltraCommon.Logger;
using Microsoft.Extensions.Configuration;

    namespace EltraMasterWatchDog
    {
        public class WatchdogSettings
        {
            #region Private fields

            private static IConfiguration _configuration;
            private LoggingSettings _loggingSettings;

            #endregion

            #region Constructors

            public WatchdogSettings()
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

        public string AgentLogin
            {
                get
                {
                    return Configuration["AgentLogin"];
                }
                set
                {
                    Configuration["AgentLogin"] = value;
                }
            }

            public string AgentPassword
            {
                get
                {
                    return Configuration["AgentPassword"];
                }
                set
                {
                    Configuration["AgentPassword"] = value;
                }
            }

        public string DeviceLogin
        {
            get
            {
                return Configuration["DeviceLogin"];
            }
            set
            {
                Configuration["DeviceLogin"] = value;
            }
        }

        public string DevicePassword
        {
            get
            {
                return Configuration["DevicePassword"];
            }
            set
            {
                Configuration["DevicePassword"] = value;
            }
        }

        public string MasterProcess
        {
            get
            {
                return Configuration["MasterProcess"];
            }
            set
            {
                Configuration["MasterProcess"] = value;
            }
        }

        public int MaxInactivityTimeInMinutes
        {
            get
            {
                return int.Parse(Configuration["MaxInactivityTimeInMinutes"]);
            }
            set
            {
                Configuration["MaxInactivityTimeInMinutes"] = $"{value}";
            }
        }

        public bool RespawnMaster
        {
            get
            {
                return bool.Parse(Configuration["RespawnMaster"]);
            }
            set
            {
                Configuration["RespawnMaster"] = $"{value}";
            }
        }


        public LoggingSettings Logging
            {
                get => _loggingSettings ?? (_loggingSettings = new LoggingSettings(Configuration));
            }

            private IConfiguration Configuration
            {
                get => _configuration ?? (_configuration = new ConfigurationBuilder()
                                                                .SetBasePath(Directory.GetCurrentDirectory())
                                                                .AddJsonFile("appsettings.json", true, true)
                                                                .Build());
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

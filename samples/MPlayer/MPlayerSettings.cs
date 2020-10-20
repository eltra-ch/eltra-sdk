using System.IO;
using Microsoft.Extensions.Configuration;

namespace MPlayerMaster
{
    public class MPlayerSettings
    {
        #region Private fields

        private static IConfiguration _configuration;

        #endregion

        #region Constructors

        public MPlayerSettings()
        {   
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

        public string Login
        {
            get
            {
                return Configuration["Login"];
            }
            set
            {
                Configuration["Login"] = value;
            }
        }

        public string Alias
        {
            get
            {
                return Configuration["Alias"];
            }
            set
            {
                Configuration["Alias"] = value;
            }
        }

        public string LoginPasswd
        {
            get
            {
                return Configuration["LoginPasswd"];
            }
            set
            {
                Configuration["LoginPasswd"] = value;
            }
        }

        public string AliasPasswd
        {
            get
            {
                return Configuration["AliasPasswd"];
            }
            set
            {
                Configuration["AliasPasswd"] = value;
            }
        }

        public string XddFile
        {
            get
            {
                return Configuration["XddFile"];
            }
            set
            {
                Configuration["XddFile"] = value;
            }
        }

        public string AppArgs
        {
            get
            {
                return Configuration["AppArgs"];
            }
            set
            {
                Configuration["AppArgs"] = value;
            }
        }

        public string LogLevels
        {
            get
            {
                return Configuration["LogLevels"];
            }
            set
            {
                Configuration["LogLevels"] = value;
            }
        }
        public string LogOutputs
        {
            get
            {
                return Configuration["LogOutputs"];
            }
            set
            {
                Configuration["LogOutputs"] = value;
            }
        }

        private IConfiguration Configuration
        {
            get => _configuration ?? (_configuration = new ConfigurationBuilder()
                                                            .SetBasePath(Directory.GetCurrentDirectory())
                                                            .AddJsonFile("appsettings.json", true, true)
                                                            .Build());
        }

        #endregion
    }
}

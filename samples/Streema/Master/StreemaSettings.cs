using System.IO;
using Microsoft.Extensions.Configuration;

namespace StreemaMaster
{
    public class StreemaSettings
    {
        #region Private fields

        private static IConfiguration _configuration;

        #endregion

        #region Constructors

        public StreemaSettings()
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

        public string AppName
        {
            get
            {
                return Configuration["AppName"];
            }
            set
            {
                Configuration["AppName"] = value;
            }
        }
        public string AppPath
        {
            get
            {
                return Configuration["AppPath"];
            }
            set
            {
                Configuration["AppPath"] = value;
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

        public string NavigoPluginsPath
        {
            get
            {
                return Configuration["NavigoPluginsPath"];
            }
            set
            {
                Configuration["NavigoPluginsPath"] = value;
            }
        }

        public string PlayUrl
        {
            get
            {
                var result = Configuration["PlayUrl"];

                if (!result.EndsWith('/'))
                {
                    result += "/";
                }

                return result;
            }
        }

        public bool IsWebKitProcess
        {
            get
            {
                bool result = false;
                var webKitProcessString = Configuration["WebKitProcess"];

                if(bool.TryParse(webKitProcessString, out bool webKitProcess))
                {
                    result = webKitProcess;
                }

                return result;
            }
        }

        public bool UseIFrameDelegation
        {
            get
            {
                bool result = false;
                var iFrameDelegation = Configuration["IFrameDelegation"];

                if (bool.TryParse(iFrameDelegation, out bool val))
                {
                    result = val;
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

        #endregion
    }
}

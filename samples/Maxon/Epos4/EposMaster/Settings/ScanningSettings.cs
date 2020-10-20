using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace EposMaster.Settings
{
    public class ScanningSettings
    {
        #region Private fields

        private readonly IConfiguration _config;
        private SkipScanningSettings _skipSettings;

        #endregion

        #region Constructors

        public ScanningSettings(IConfiguration config)
        {
            _config = config;
        }

        #endregion

        #region Properties

        public SkipScanningSettings Skip => _skipSettings ?? (_skipSettings = new SkipScanningSettings(_config));

        public List<string> DeviceNames
        {
            get
            {
                var nameList = _config["Scanning:DeviceNames"];

                var result = RemoveSeparators(nameList);

                return result;
            }
            set
            {
                var namesList = value;

                foreach (var name in namesList)
                {
                    if(!DeviceNames.Contains(name))
                    {
                        _config["Scanning:DeviceNames"] += name;
                        _config["Scanning:DeviceNames"] += ",";
                    }
                }
            }
        }

        #endregion

        #region Methods

        private List<string> RemoveSeparators(string settingValue)
        {
            var result = new List<string>();
            
            if(settingValue!=null)
            {
                result = settingValue.Split(new[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return result;
        }
                
        #endregion
    }
}

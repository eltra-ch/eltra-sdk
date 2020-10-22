using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EposMaster.Settings
{
    public class SkipScanningSettings
    {
        private readonly IConfiguration _config;

        public SkipScanningSettings(IConfiguration config)
        {
            _config = config;
        }

        public List<string> ProtocolStacks
        {
            get
            {
                var nameList = _config["Scanning:Skip:ProtocolStacks"];

                return RemoveSeparators(nameList);
            }
            set
            {
                var namesList = value;

                foreach (var name in namesList)
                {
                    if(!ProtocolStacks.Contains(name))
                    {
                        _config["Scanning:Skip:ProtocolStacks"] += name;
                        _config["Scanning:DeviceNames"] += ",";
                    }
                }
            }
        }

        public List<string> Interfaces
        {
            get
            {
                var nameList = _config["Scanning:Skip:InterfaceNames"];

                var result = RemoveSeparators(nameList);

                return result;
            }
            set
            {
                var namesList = value;

                foreach(var name in namesList)
                {
                    if (!Interfaces.Contains(name))
                    {
                        _config["Scanning:Skip:InterfaceNames"] += name;
                        _config["Scanning:DeviceNames"] += ",";
                    }
                }                
            }
        }

        private List<string> RemoveSeparators(string settingValue)
        {
            var result = new List<string>();

            if (settingValue != null)
            {
                result = settingValue.Split(new[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return result;
        }

        internal bool SkipInterface(string interfaceName)
        {
            bool result = false;

            if(Interfaces.Count>0)
            {
                result = Interfaces.Contains(interfaceName);
            }
            
            return result;
        }

        internal bool SkipProtocolStack(string protocolStackName)
        {
            bool result = false;

            if (ProtocolStacks.Count > 0)
            {
                result = ProtocolStacks.Contains(protocolStackName);
            }

            return result;
        }
    }
}

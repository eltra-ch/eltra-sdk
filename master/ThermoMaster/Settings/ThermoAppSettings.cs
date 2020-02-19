using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace ThermoMaster.Settings
{
    public class DeviceSettings
    {
        #region Private fields

        private readonly IConfiguration _configuration;
        private static Dht22Settings _dht22Settings;
        private static Bmp180Settings _bmp180Settings;

        #endregion

        #region Constructors

        public DeviceSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region Properties

        public Dht22Settings Dht22Settings
        {
            get => _dht22Settings ?? (_dht22Settings = new Dht22Settings(_configuration));
        }

        public Bmp180Settings Bmp180Settings
        {
            get => _bmp180Settings ?? (_bmp180Settings = new Bmp180Settings(_configuration));
        }

        public List<int> Pins
        {
            get
            {
                var nameList = _configuration["Device:Pins"];
                var result = new List<int>();
                var pinsList = RemoveSeparators(nameList);

                foreach(var pin in pinsList)
                {
                    result.Add(Convert.ToInt32(pin));
                }

                return result;
            }            
        }

        public ulong SerialNumber
        {
            get
            {
                var serialNumberText = _configuration["Device:SerialNumber"];
                ulong result;

                if (serialNumberText.StartsWith("0x"))
                {
                    ulong.TryParse(serialNumberText.Substring(2), NumberStyles.HexNumber, null, out result);
                }
                else
                {
                    result = ulong.Parse(serialNumberText);
                }

                return result;
            }
        }

        public string XddFile => _configuration["Device:XddFile"];

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

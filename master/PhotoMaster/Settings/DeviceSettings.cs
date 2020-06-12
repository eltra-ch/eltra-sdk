using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace PhotoMaster.Settings
{
    public class DeviceSettings
    {
        #region Private fields

        private readonly IConfiguration _configuration;

        #endregion

        #region Constructors

        public DeviceSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region Properties

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
    }
}

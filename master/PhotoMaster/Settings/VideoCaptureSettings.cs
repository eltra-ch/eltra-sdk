using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace PhotoMaster.Settings
{
    public class VideoCaptureSettings
    {
        #region Private fields

        private readonly IConfiguration _configuration;

        #endregion

        #region Constructors

        public VideoCaptureSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region Properties

        public int DeviceId
        {
            get
            {
                var deviceIdText = _configuration["VideoCapture:DeviceId"];
                int result;

                if (deviceIdText.StartsWith("0x"))
                {
                    int.TryParse(deviceIdText.Substring(2), NumberStyles.HexNumber, null, out result);
                }
                else
                {
                    result = int.Parse(deviceIdText);
                }

                return result;
            }
        }

        public int AppId
        {
            get
            {
                var appIdText = _configuration["VideoCapture:AppId"];
                int result;

                if (appIdText.StartsWith("0x"))
                {
                    int.TryParse(appIdText.Substring(2), NumberStyles.HexNumber, null, out result);
                }
                else
                {
                    result = int.Parse(appIdText);
                }

                return result;
            }
        }

        #endregion
    }
}

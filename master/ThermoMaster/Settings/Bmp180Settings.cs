using Microsoft.Extensions.Configuration;

namespace ThermoMaster.Settings
{    
    public class Bmp180Settings
    {
        #region Private fields

        private readonly IConfiguration _configuration;

        #endregion

        #region Constructors

        public Bmp180Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region Properties

        public bool Active
        {
            get
            {
                var text = _configuration["Device:Bmp180:Active"];

                bool.TryParse(text, out bool result);

                return result;
            }
        }

        public TempSensorUsage Usage
        {
            get
            {
                var result = TempSensorUsage.Undefined;
                var text = _configuration["Device:Bmp180:Usage"];

                text = text.ToLower();

                switch(text)
                {
                    case "external": result = TempSensorUsage.External; break;
                    case "internal": result = TempSensorUsage.Internal; break;
                }

                return result;
            }
        }

        public AltitudeSettings Altitude
        {
            get
            {
                var result = new AltitudeSettings(_configuration);

                return result;
            }
        }


        #endregion
    }
}

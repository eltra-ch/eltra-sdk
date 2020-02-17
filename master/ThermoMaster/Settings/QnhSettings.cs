using Microsoft.Extensions.Configuration;

namespace ThermoMaster.Settings
{
    public class AltitudeSettings
    {
        #region Private fields

        private readonly IConfiguration _configuration;

        #endregion

        #region Constructors

        public AltitudeSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public double Station
        {
            get
            {
                double result = double.NaN;
                var text = _configuration["Device:Bmp180:AltitudeStation"];

                if(double.TryParse(text, out var val))
                {
                    result = val;
                }

                return result;
            }
        }

        public string Unit
        {
            get
            {
                string result = string.Empty;
                var text = _configuration["Device:Bmp180:AltitudeUnit"];

                result = text;

                return result;
            }
        }

        #endregion
    }
}

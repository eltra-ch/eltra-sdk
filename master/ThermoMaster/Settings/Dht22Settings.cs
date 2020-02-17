using Microsoft.Extensions.Configuration;
using System;

namespace ThermoMaster.Settings
{
    public class Dht22Settings
    {
        #region Private fields

        private readonly IConfiguration _configuration;

        #endregion

        #region Constructors

        public Dht22Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region Properties
        
        public int InternalPin
        {
            get
            {
                var pinName = _configuration["Device:Dht22:InternalPin"];

                return Convert.ToInt32(pinName);
            }
        }

        public int ExternalPin
        {
            get
            {
                var pinName = _configuration["Device:Dht22:ExternalPin"];

                return Convert.ToInt32(pinName);
            }
        }

        #endregion
    }
}

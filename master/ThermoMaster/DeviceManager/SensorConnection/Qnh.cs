using System;
using ThermoMaster.Settings;

namespace ThermoMaster.DeviceManager.SensorConnection
{
    class Qnh
    {
        private Bmp180Settings _bmp180Settings;

        public Qnh(Bmp180Settings bmp180Settings)
        {
            _bmp180Settings = bmp180Settings;
        }

        private double CalculateQnh(double pressure, double stationAltitude, string unit)
        {
            double result = 0;

            if (unit == "m")
            {
                result = pressure / Math.Pow((1 - (stationAltitude / 44330)), 5.255);
            }
            else if (unit == "ft")
            {
                result = pressure / Math.Pow((1 - (stationAltitude / 145437.86)), 5.255);
            }

            return result;
        }

        public double GetValue(double pressure)
        {
            return CalculateQnh(pressure, _bmp180Settings.Altitude.Station, _bmp180Settings.Altitude.Unit);
        }
    }
}

using System;
using System.IO;
using System.Globalization;

namespace ThermoMaster.DeviceManager.SensorConnection.DataSource
{
    public class Bme280DataSource
    {
        #region Private fields

        private string _filename;
        private DateTime _date;
        private double _temperature;
        private double _pressure;

        #endregion

        #region Constructors

        public Bme280DataSource(string filename)
        {
            _filename = filename;            
        }

        #endregion

        #region Methods

        private bool ParseFile()
        {
            bool result = false;

            try
            {
                if (File.Exists(_filename))
                {
                    using (var reader = new StreamReader(_filename))
                    {
                        String line = "";

                        while (!reader.EndOfStream)
                        {
                            line = reader.ReadLine();
                        }

                        if (line != "")
                        {
                            var values = line.Split(';');

                            _temperature = double.Parse(values[1], CultureInfo.InvariantCulture);
                            _pressure = double.Parse(values[2], CultureInfo.InvariantCulture);
                            _date = DateTime.Parse(values[0]);

                            result = true;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }

        public bool ReadBme280(out double temp, out double pressure, out DateTime date)
        {
            bool result = ParseFile();

            temp = _temperature;
            pressure = _pressure;
            date = _date;

            return result;
        }

        #endregion
    }
}
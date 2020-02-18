using System;
using System.IO;
using System.Globalization;

namespace ThermoMaster.DeviceManager.SensorConnection.DataSource
{
    public class Dht11DataSource
    {
        #region Private fields

        private string _filename;
        private DateTime _date;
        private double _temperature;
        private double _humidity;

        #endregion

        #region Constructors

        public Dht11DataSource(string filename)
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
                            _humidity = double.Parse(values[2], CultureInfo.InvariantCulture);
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

        public bool ReadDht11(out double temp, out double humid, out DateTime date)
        {
            bool result = ParseFile();

            temp = _temperature;
            humid = _humidity;
            date = _date;

            return result;
        }

        #endregion
    }
}

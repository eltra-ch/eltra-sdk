using EltraCommon.Math;
using System;

namespace ThermoMaster.DeviceManager.SensorConnection
{
    class ThermoSensorsSample
    {
        #region Private fields

        private Kalman _kalmanFilterT;
        private Kalman _kalmanFilterH;
        private SampleSource _source;

        #endregion

        #region Constructors

        public ThermoSensorsSample()
        {
            Timestamp = DateTime.Now;
            _source = SampleSource.Undefined;
        }

        public ThermoSensorsSample(ThermoSensorsSample sample)
        {
            _source = sample.Source;

            Count = sample.Count;

            Temperature = sample.Temperature;
            TemperatureAveraged = sample.TemperatureAveraged;

            Humidity = sample.Humidity;
            HumidityAveraged = sample.HumidityAveraged;
                        
            Pressure = sample.Pressure;

            Timestamp = sample.Timestamp;
        }

        #endregion

        #region Properties

        public int Count { get; set; }

        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        
        public double TemperatureAveraged { get; private set; }
        public double HumidityAveraged { get; private set; }
        public DateTime Timestamp { get; set; }
        public SampleSource Source
        {
            get => _source;
        }

        #endregion

        #region Methods

        public void Set(SampleSource source, double temperature, double humidity)
        {
            _source = source;

            Temperature = temperature;

            if(source == SampleSource.dht11)
            {
                Humidity = humidity; 
            }
            else
            {
                Pressure = humidity;
            }
                        
            if(Count==0)
            {
                _kalmanFilterT = new Kalman(0.125, 32, 1023, temperature);
                
                TemperatureAveraged = temperature;

                if (source == SampleSource.dht11)
                {
                    _kalmanFilterH = new Kalman(0.125, 32, 1023, humidity);
                    HumidityAveraged = humidity;
                }
            }
            else
            {
                TemperatureAveraged = _kalmanFilterT.GetFilteredValue(temperature);
                TemperatureAveraged = Math.Round(TemperatureAveraged, 1);

                if (source == SampleSource.dht11)
                {
                    HumidityAveraged = _kalmanFilterH.GetFilteredValue(humidity);
                    HumidityAveraged = Math.Round(HumidityAveraged, 1);
                }
            }

            Timestamp = DateTime.Now;

            Count++;
        }

        #endregion
    }
}

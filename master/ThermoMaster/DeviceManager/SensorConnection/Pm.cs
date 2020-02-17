using System;

namespace ThermoMaster.DeviceManager.SensorConnection
{
    class Pm
    {
        private double _humidity;

        public Pm(double humidity, float pm25, float pm10)
        {
            _humidity = humidity;

            RawPm25 = pm25;
            RawPm10 = pm10;
        }

        public float RawPm25 { get; set; }
        public float RawPm10 { get; set; }

        public float Pm25 { get; set; }
        public float Pm10 { get; set; }

        //Correction algorythm thanks to help of Zbyszek Kiliański (Krakow Zdroj)
        private float NormalizePM25(float pm25, double humidity)
        {
            float factor = (float)(Math.Pow((humidity / 100.0), 8.60068));

            return (float)(pm25 / (1.0 + 0.48756 * factor));
        }
        
        private float NormalizePM10(float pm10, double humidity)
        {
            float factor = (float)(Math.Pow((humidity / 100.0), 5.83411));

            return (float)(pm10 / (1.0 + 0.81559 * factor));
        }

        public void Normalize()
        {
            NormalizePM25(RawPm25, _humidity);
            NormalizePM10(RawPm10, _humidity);
        }
    }
}

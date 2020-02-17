using System;

namespace EltraNavigo.Views.Devices.Thermo.History
{
    public class SensorHistoryItem
    {
        public SensorHistoryItem(double value, DateTime timestamp)
        {
            Value = value;
            Timestamp = timestamp;
        }

        public double Value { get; set; }

        public DateTime Timestamp { get; set; }
    }
}

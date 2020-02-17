using EltraNavigo.Controls;
using System;

namespace EltraNavigo.Views.Devices.Thermo.History
{
    public class TemperatureHistory : BaseViewModel
    {
        private string _value;
        private string _time;

        public TemperatureHistory(double value, DateTime time)
        {
            _value = value.ToString();
            _time = time.ToString();
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

    }
}

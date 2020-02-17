using System;
using System.Globalization;
using Xamarin.Forms;

namespace EltraNavigo.Views.Devices.Thermo.Overview.Converters
{
    public class RelayStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color result = Color.FromHex("#2196F3");

            if(value is ushort val)
            {
                if(val == 0)
                {
                    result = Color.FromHex("#e25822");
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

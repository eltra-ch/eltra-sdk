using System.Globalization;
using Xamarin.Forms;
using System;

namespace EltraXamCommon.Controls.Converters
{
    public class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !System.Convert.ToBoolean(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !System.Convert.ToBoolean(value);
        }
    }
}

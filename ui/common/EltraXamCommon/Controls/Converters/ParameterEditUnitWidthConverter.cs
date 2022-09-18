using System;
using System.Globalization;
using Xamarin.Forms;

namespace EltraXamCommon.Controls.Converters
{
    public class ParameterEditUnitWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            if(value is double)
            {
                var width = System.Convert.ToDouble(value);

                if(width > 0)
                {
                    result = true;
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

using System;
using System.Globalization;
using Xamarin.Forms;

namespace EltraXamCommon.Controls.Converters
{
    public class ParameterEditFontValidationColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color result = Color.Black;

            if (value is bool bv)
            {
                if(!bv)
                {
                    result = Color.Red;
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

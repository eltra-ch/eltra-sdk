﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace EltraXamCommon.Controls.Converters
{
    public class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            if(value is string stringValue)
            {
                result = !string.IsNullOrEmpty(stringValue);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

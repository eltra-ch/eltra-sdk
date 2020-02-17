using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace EltraNavigo.Views.Converters
{
    public class StatusWordToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            var assemblyName = assembly.GetName();

            var imageUndefined = ImageSource.FromResource($"{assemblyName.Name}.Resources.led_undefined.png");
            var imageActive = ImageSource.FromResource($"{assemblyName.Name}.Resources.led_active.png");
            var imageInactive = ImageSource.FromResource($"{assemblyName.Name}.Resources.led_inactive.png");

            ImageSource result = imageUndefined;

            if (value != null)
            {
                ushort statusWord = System.Convert.ToUInt16(value);
                var statusBits = new BitArray(BitConverter.GetBytes(statusWord));
                var bitNumber = System.Convert.ToUInt16(parameter);

                if (statusBits[bitNumber])
                {
                    result = imageActive;
                }
                else
                {
                    result = imageInactive;
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

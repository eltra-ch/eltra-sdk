using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace EltraNavigoStreema.Views.Devices.Streema.StreemaControl.Converters
{
    class StatusWordToImageConverter : IValueConverter
    {
        internal enum StatusWordEnums
        {
            Undefined = 0x0,
            Waiting = 0x0001,
            PendingExecution = 0x0010,
            ExecutedSuccessfully = 0x0020,
            ExecutionFailed = 0x8000
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            var assemblyName = assembly.GetName();

            var imageUndefined = ImageSource.FromResource($"{assemblyName.Name}.Resources.state_grey.png");
            var imageSuccess = ImageSource.FromResource($"{assemblyName.Name}.Resources.state_green.png");
            var imageFailed = ImageSource.FromResource($"{assemblyName.Name}.Resources.state_red.png");
            var imageExecuting = ImageSource.FromResource($"{assemblyName.Name}.Resources.state_blue.png");

            ImageSource result = imageUndefined;

            if (value != null)
            {
                StatusWordEnums statusWord = (StatusWordEnums)System.Convert.ToUInt16(value);

                switch(statusWord)
                {
                    case StatusWordEnums.Undefined:
                        result = imageUndefined;
                        break;
                    case StatusWordEnums.PendingExecution:
                        result = imageExecuting;
                        break;
                    case StatusWordEnums.ExecutedSuccessfully:
                        result = imageSuccess;
                        break;
                    case StatusWordEnums.ExecutionFailed:
                        result = imageFailed;
                        break;
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

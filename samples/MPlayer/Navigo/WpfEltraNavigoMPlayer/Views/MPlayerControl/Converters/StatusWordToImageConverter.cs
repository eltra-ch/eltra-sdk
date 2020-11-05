using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace EltraNavigoMPlayer.Views.MPlayerControl.Converters
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

            var imageUndefined = new BitmapImage(new Uri(@$"pack://application:,,,/{assemblyName};component/Resources/state_grey.png", UriKind.Absolute));
            var imageSuccess = new BitmapImage(new Uri(@$"pack://application:,,,/{assemblyName};component/Resources/state_green.png", UriKind.Absolute));
            var imageFailed = new BitmapImage(new Uri(@$"pack://application:,,,/{assemblyName};component/Resources/state_red.png", UriKind.Absolute));
            var imageExecuting = new BitmapImage(new Uri(@$"pack://application:,,,/{assemblyName};component/Resources/state_blue.png", UriKind.Absolute));

            BitmapImage result = imageUndefined;

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

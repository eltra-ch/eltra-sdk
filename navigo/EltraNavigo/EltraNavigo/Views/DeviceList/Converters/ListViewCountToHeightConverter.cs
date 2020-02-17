using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;


namespace EltraNavigo.Views.DeviceList.Converters
{
    class ListViewCountToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const double defaultRowHeight = 164;
            const double androidRowHeight = 164;
            const double uwpRowHeight = 164;

            double result = 0;

            if(value is List<DeviceViewModel> devices)
            {
                var platform = Xamarin.Forms.Device.RuntimePlatform;

                double rowHeight;

                switch(platform)
                {
                    case Xamarin.Forms.Device.Android:
                        rowHeight = androidRowHeight;
                        break;
                    case Xamarin.Forms.Device.UWP:
                        rowHeight = uwpRowHeight;
                        break;
                    default:
                        rowHeight = defaultRowHeight;
                        break;
                }

                result = devices.Count * rowHeight;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

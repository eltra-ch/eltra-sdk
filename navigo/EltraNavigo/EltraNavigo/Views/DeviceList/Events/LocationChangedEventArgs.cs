using System;
using Xamarin.Forms.Maps;

namespace EltraNavigo.Views.DeviceList.Events
{
    public class LocationChangedEventArgs : EventArgs 
    {
        public Position Position { get; set; }
    }
}

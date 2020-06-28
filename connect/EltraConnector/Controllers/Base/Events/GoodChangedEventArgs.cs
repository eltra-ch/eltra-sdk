using System;

namespace EltraConnector.Controllers.Base.Events
{
    public class GoodChangedEventArgs : EventArgs
    {
        public bool Good { get; set; }
    }
}
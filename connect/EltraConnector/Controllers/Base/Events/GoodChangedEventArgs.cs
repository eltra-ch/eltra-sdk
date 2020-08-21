using System;

namespace EltraConnector.Controllers.Base.Events
{
    /// <summary>
    /// GoodChangedEventArgs
    /// </summary>
    public class GoodChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Good
        /// </summary>
        public bool Good { get; set; }
    }
}
using System;
using EltraConnector.Master.Definitions;

namespace EltraConnector.Master.Events
{
    /// <summary>
    /// MasterStatusEventArgs
    /// </summary>
    public class MasterStatusEventArgs : EventArgs
    {
        /// <summary>
        /// Master status
        /// </summary>
        public MasterStatus Status { get; set; }
    }
}

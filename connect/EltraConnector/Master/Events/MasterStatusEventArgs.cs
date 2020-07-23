using System;
using EltraConnector.Master.Definitions;

namespace EltraConnector.Master.Events
{
    public class MasterStatusEventArgs : EventArgs
    {
        public MasterStatus Status { get; set; }
    }
}

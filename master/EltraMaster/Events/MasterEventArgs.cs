using System;
using EltraMaster.Status;

namespace EltraMaster.Events
{
    public class MasterStatusEventArgs : EventArgs
    {
        public MasterStatus Status { get; set; }
    }
}

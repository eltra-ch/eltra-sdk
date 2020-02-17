using ThermoMaster.Status;
using System;

namespace ThermoMaster.Events
{
    public class MasterStatusEventArgs : EventArgs
    {
        public MasterStatus Status { get; set; }
    }
}

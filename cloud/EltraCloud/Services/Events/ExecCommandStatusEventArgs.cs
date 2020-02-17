using EltraCloudContracts.Contracts.CommandSets;
using System;

#pragma warning disable CS1591

namespace EltraCloud.Services.Events
{
    public class ExecCommandStatusEventArgs : EventArgs
    {
        public ExecuteCommandStatus Status { get; set; }
    }
}

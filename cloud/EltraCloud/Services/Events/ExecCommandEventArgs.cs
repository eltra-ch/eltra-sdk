using EltraCloudContracts.Contracts.CommandSets;
using System;

#pragma warning disable CS1591

namespace EltraCloud.Services.Events
{
    public class ExecCommandEventArgs : EventArgs
    {
        public ExecuteCommand ExecuteCommand { get; set; }
    }
}

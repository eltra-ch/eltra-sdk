using System;
using EltraCloudContracts.Contracts.CommandSets;

namespace EltraConnector.UserAgent.Events
{
    public class ExecuteCommanderEventArgs : EventArgs
    {
        public string CommandUuid { get; set; }

        public ExecCommandStatus Status { get; set; }

        public DeviceCommand Command { get; set; }
    }
}

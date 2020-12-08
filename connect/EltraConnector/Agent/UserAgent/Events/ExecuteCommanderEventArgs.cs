using System;
using EltraCommon.Contracts.CommandSets;

namespace EltraConnector.UserAgent.Events
{
    /// <summary>
    /// ExecuteCommanderEventArgs
    /// </summary>
    public class ExecuteCommanderEventArgs : EventArgs
    {
        /// <summary>
        /// Command uuid
        /// </summary>
        public string CommandUuid { get; set; }

        /// <summary>
        /// Command status
        /// </summary>
        public ExecCommandStatus Status { get; set; }

        /// <summary>
        /// {DeviceCommand} instance
        /// </summary>
        public DeviceCommand Command { get; set; }
    }
}

using EltraConnector.UserAgent.Definitions;
using System;

namespace EltraConnector.Agent.UserAgent.Events
{

    /// <summary>
    /// AgentStatusEventArgs - Agent status event arguments
    /// </summary>
    public class AgentStatusEventArgs : EventArgs
    {
        /// <summary>
        /// {AgentStatus} - agent status
        /// </summary>
        public AgentStatus Status { get; set; }
    }
}
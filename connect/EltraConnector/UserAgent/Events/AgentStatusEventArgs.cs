using EltraConnector.UserAgent.Definitions;
using System;

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
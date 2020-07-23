using EltraConnector.UserAgent.Definitions;
using System;

public class AgentStatusEventArgs : EventArgs
{
    public AgentStatus Status { get; set; }
}
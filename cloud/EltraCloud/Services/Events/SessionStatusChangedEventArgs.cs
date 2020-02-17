using EltraCloudContracts.Contracts.Sessions;
using System;

namespace EltraCloud.Services.Events
{
    /// <summary>
    /// SessionStatusChanged event arguments
    /// </summary>
    public class SessionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Session Uuid
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// Session Status
        /// </summary>
        public SessionStatus Status { get; set; }
}
}

using EltraCommon.Contracts.Devices;
using System;

namespace EltraConnector.Controllers.Events
{
    /// <summary>
    /// ParameterUpdateEventArgs
    /// </summary>
    public class ParameterUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Device
        /// </summary>
        public EltraDevice Device { get; set; }

        /// <summary>
        /// Update Result
        /// </summary>
        public bool Result { get; set; }
    }
}

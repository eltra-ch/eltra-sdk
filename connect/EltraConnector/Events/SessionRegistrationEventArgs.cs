using System;
using EltraCommon.Contracts.Channels;

namespace EltraConnector.Events
{
    /// <summary>
    /// ChannelRegistrationEventArgs
    /// </summary>
    public class ChannelRegistrationEventArgs : EventArgs
    {
        /// <summary>
        /// Channel
        /// </summary>
        public Channel Channel { get; set; }

        /// <summary>
        /// Success
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; set; }
    }
}

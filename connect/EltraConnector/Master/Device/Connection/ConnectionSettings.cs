namespace EltraConnector.Master.Device.Connection
{
    /// <summary>
    /// ConnectionSettings
    /// </summary>
    public class ConnectionSettings
    {
        /// <summary>
        /// ConnectionSettings
        /// </summary>
        public ConnectionSettings()
        {
            UpdateInterval = 60;
            Timeout = 120;
        }

        /// <summary>
        /// UpdateInterval
        /// </summary>
        public uint UpdateInterval { get; set; }
        /// <summary>
        /// Timeout
        /// </summary>
        public uint Timeout { get; set; }
    }
}

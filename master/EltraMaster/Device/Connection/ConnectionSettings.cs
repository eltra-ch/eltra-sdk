namespace EltraMaster.Device.Connection
{
    public class ConnectionSettings
    {
        public ConnectionSettings()
        {
            UpdateInterval = 60;
            Timeout = 120;
        }

        public uint UpdateInterval { get; set; } 
        public uint Timeout { get; set; }
    }
}

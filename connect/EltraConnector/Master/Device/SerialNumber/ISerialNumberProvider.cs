namespace EltraConnector.Master.Device.SerialNumber
{
    /// <summary>
    /// ISerialNumberProvider
    /// </summary>
    public interface ISerialNumberProvider
    {
        /// <summary>
        /// ReadSerialNumber
        /// </summary>
        /// <returns></returns>
        ulong ReadSerialNumber();
    }
}

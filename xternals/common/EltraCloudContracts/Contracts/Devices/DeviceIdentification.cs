using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.Devices
{
    [DataContract]
    public class DeviceIdentification
    {
        #region Properties

        [DataMember]
        public ulong SerialNumber { get; set; }

        #endregion
    }
}

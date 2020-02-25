using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.Devices
{
    [DataContract]
    public class DeviceIdentification
    {
        #region Properties

        [DataMember]
        public ulong SerialNumber { get; set; }

        [DataMember]
        public string Name { get; set; }

        #endregion

        #region Methods

        public virtual bool Read()
        {
            return false;
        }

        #endregion
    }
}

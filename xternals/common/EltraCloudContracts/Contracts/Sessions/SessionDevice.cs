using System.Runtime.Serialization;

using EltraCloudContracts.Contracts.Devices;

namespace EltraCloudContracts.Contracts.Sessions
{
    [DataContract]
    public class SessionDevice
    {
        #region Properties

        [DataMember]
        public string SessionUuid { get; set; }

        [DataMember]
        public EltraDevice Device { get; set; }

        #endregion
    }
}

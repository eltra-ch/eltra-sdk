using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.Devices
{
    [DataContract]
    public class DeviceLock
    {
        [DataMember]
        public string AgentUuid { get; set; }

        [DataMember]
        public ulong SerialNumber { get; set; }
    }
}

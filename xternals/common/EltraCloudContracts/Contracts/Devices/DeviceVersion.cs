using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.Devices
{
    [DataContract]
    public class DeviceVersion
    {
        [DataMember]
        public ushort HardwareVersion { get; set; }
        [DataMember]
        public ushort SoftwareVersion { get; set; }
        [DataMember]
        public ushort ApplicationNumber { get; set; }
        [DataMember]
        public ushort ApplicationVersion { get; set; }

        public override string ToString()
        {
            return $"0x{HardwareVersion:X2} 0x{SoftwareVersion:X2} 0x{ApplicationNumber:X2} 0x{ApplicationVersion:X2}";
        }
    }
}

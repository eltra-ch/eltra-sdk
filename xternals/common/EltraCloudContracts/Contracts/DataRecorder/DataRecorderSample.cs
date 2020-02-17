using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.DataRecorder
{
    [DataContract]
    public class DataRecorderSample
    {
        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public uint Timestamp { get; set; }
    }
}

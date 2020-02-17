using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.Sessions
{
    [DataContract]
    public class SessionIdentification
    {
        [DataMember]
        public string Uuid { get; set; }
    }
}

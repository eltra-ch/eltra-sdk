using EltraCommon.Contracts.Users;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EltraConnector.Transport.Udp.Contracts
{
    [DataContract]
    internal class UdpRequest
    {
        public UdpRequest()
        {
            Header = "ABE3";
        }

        [DataMember]
        public string Header { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public IPEndPoint Endpoint { get; set; }

        [DataMember]
        public UserIdentity Identity { get; set; }

        [DataMember]
        public string TypeName { get; set; }
        
        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public string Checksum { get; set; }
    }
}

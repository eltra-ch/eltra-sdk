using EltraCommon.Contracts.Users;
using System.Net;
using System.Runtime.Serialization;

namespace EltraConnector.Transport.Udp.Contracts
{
    [DataContract]
    internal class UdpRequest
    {
        [IgnoreDataMember]
        public IPEndPoint Endpoint { get; set; }

        [DataMember]
        public UserIdentity Identity { get; set; }

        [DataMember]
        public string TypeName { get; set; }
        
        [DataMember]
        public string Data { get; set; }
    }
}

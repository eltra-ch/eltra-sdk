using EltraCommon.Contracts.Users;
using System.Net;

namespace EltraConnector.Transport.Udp
{
    class EndpointStoreEntry
    {
        public UserIdentity Identity { get; set; }
        public IPEndPoint Endpoint { get; set; }
    }
}

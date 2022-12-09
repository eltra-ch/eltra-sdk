using EltraCommon.Contracts.Users;
using EltraCommon.Contracts.Ws;
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
            Header = DefaultHeader;
        }

        /// <summary>
        /// DefaultHeader
        /// </summary>
        public static string DefaultHeader = "ABE3";

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

        public bool IsControlMessage()
        {
            bool result = false;
            var msg = Data.Trim(new char[] { '\"' });

            if (msg == WsMessageAck.RawData || msg == WsMessageKeepAlive.RawData)
            {
                result = true;
            }

            return result;
        }
    }
}

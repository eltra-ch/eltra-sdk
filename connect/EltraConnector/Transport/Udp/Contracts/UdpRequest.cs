using EltraCommon.Contracts.Users;
using EltraCommon.Contracts.Ws;
using System.ComponentModel;
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
            Discriminator = DefaultDiscriminator;
        }

        /// <summary>
        /// DefaultHeader
        /// </summary>
        private const string DefaultDiscriminator = "UdpRequest";

        [DataMember]
        [DefaultValue(DefaultDiscriminator)]
        public string Discriminator { get; set; }

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
            var trimChars = new char[] { '\"' };
            var msg = Data.Trim(trimChars);

            if (msg == WsMessageAck.RawData || msg == WsMessageKeepAlive.RawData)
            {
                result = true;
            }

            return result;
        }
    }
}

using EltraCommon.Contracts.Users;
using EltraCommon.Contracts.Ws;
using System.ComponentModel;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EltraConnector.Transport.Udp.Contracts
{
    /// <summary>
    /// UdpRequest
    /// </summary>
    [DataContract]
    public class UdpRequest
    {
        /// <summary>
        /// UdpRequest
        /// </summary>
        public UdpRequest()
        {
            Discriminator = DefaultDiscriminator;
        }

        /// <summary>
        /// DefaultHeader
        /// </summary>
        private const string DefaultDiscriminator = "UdpRequest";

        /// <summary>
        /// Discriminator
        /// </summary>
        [DataMember]
        [DefaultValue(DefaultDiscriminator)]
        public string Discriminator { get; set; }

        /// <summary>
        /// Endpoint
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public IPEndPoint Endpoint { get; set; }

        /// <summary>
        /// Identity
        /// </summary>
        [DataMember]
        public UserIdentity Identity { get; set; }

        /// <summary>
        /// TypeName
        /// </summary>
        [DataMember]
        public string TypeName { get; set; }

        /// <summary>
        /// Data
        /// </summary>
        [DataMember]
        public string Data { get; set; }

        /// <summary>
        /// Checksum
        /// </summary>
        [DataMember]
        public string Checksum { get; set; }

        /// <summary>
        /// IsControlMessage
        /// </summary>
        /// <returns></returns>
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

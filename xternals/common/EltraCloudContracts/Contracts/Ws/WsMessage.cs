using System;
using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.Ws
{
    [DataContract]
    public class WsMessage
    {
        public WsMessage()
        {
            Timestamp = DateTime.Now;
        }

        [DataMember]
        public string ChannelName { get; set; }

        [DataMember]
        public string TypeName { get; set; }

        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        public bool Equals(WsMessage obj)
        {
            bool result = false;

            if(obj != null && obj.Data == Data && obj.TypeName == TypeName)
            {
                result = true;
            }

            return result;
        }
    }
}


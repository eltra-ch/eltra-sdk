using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.Ws
{
    [DataContract]
    public class WsMessageKeepAlive : WsMessage
    {
        public WsMessageKeepAlive()
        {
            Data = "KEEPALIVE";
            TypeName = typeof(WsMessageAck).FullName;
        }
    }
}

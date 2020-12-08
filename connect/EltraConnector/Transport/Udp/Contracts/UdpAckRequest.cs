namespace EltraConnector.Transport.Udp.Contracts
{
    internal class UdpAckRequest : UdpRequest
    {
        public UdpAckRequest()
        {
            TypeName = typeof(UdpAckRequest).FullName;
            Data = "ACK";
        }
    }
}

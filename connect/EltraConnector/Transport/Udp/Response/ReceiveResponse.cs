using System.Net;

namespace EltraConnector.Transport.Udp.Response
{
    internal class ReceiveResponse
    {
        public string Text { get; set; }
        public IPEndPoint Endpoint { get; set; }
        public bool Handled { get; set; }
    }
}

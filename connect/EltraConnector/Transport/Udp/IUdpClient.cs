using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    /// <summary>
    /// IUdpClient
    /// </summary>
    public interface IUdpClient
    {
        /// <summary>
        /// Close
        /// </summary>
        void Close();
        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        void Connect(string host, int port);
        /// <summary>
        /// Create
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        void Init(string host, int port);
        /// <summary>
        /// Disconnect
        /// </summary>
        void Disconnect();
        /// <summary>
        /// ReceiveAsync
        /// </summary>
        /// <returns></returns>
        Task<UdpReceiveResult> ReceiveAsync();
        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="bytes"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        Task<int> SendAsync(byte[] datagram, int bytes, IPEndPoint endPoint);
        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        Task<int> SendAsync(byte[] datagram, int bytes);
    }
}

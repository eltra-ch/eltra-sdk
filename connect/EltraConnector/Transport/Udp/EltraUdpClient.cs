using EltraCommon.Logger;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    /// <summary>
    /// EltraUdpClient
    /// </summary>
    public class EltraUdpClient : IUdpClient, IDisposable
    {
        private UdpClient _udpClient;
        private bool disposedValue;

        /// <summary>
        /// UdpClient
        /// </summary>
        protected UdpClient UdpClient { get { return _udpClient ?? (_udpClient = new UdpClient()); } }


        /// <summary>
        /// Create
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void Init(string host, int port)
        {
            _udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(host), port));
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (UdpClient.Client.Connected)
                {
                    UdpClient.Client.Disconnect(false);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Disconnect", e);
            }
        }

        /// <summary>
        /// ReceiveAsync
        /// </summary>
        /// <returns></returns>
        public Task<UdpReceiveResult> ReceiveAsync()
        {
            return UdpClient.ReceiveAsync();
        }

        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="bytes"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public Task<int> SendAsync(byte[] datagram, int bytes, IPEndPoint endPoint)
        {
            return UdpClient.SendAsync(datagram, bytes, endPoint);
        }

        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public Task<int> SendAsync(byte[] datagram, int bytes)
        {
            return UdpClient.SendAsync(datagram, bytes);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _udpClient.Dispose();
                    _udpClient = null;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Close
        /// </summary>
        public void Close()
        {
            UdpClient.Close();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void Connect(string host, int port)
        {
            UdpClient.Connect(host, port);
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        public IUdpClient Clone()
        {
            return new EltraUdpClient();
        }
    }
}

using EltraCommon.Extensions;
using EltraCommon.Logger;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    class UdpClientWrapper : UdpClient
    {
        private CancellationTokenSource _tokenSource;
        private int _activityCounter;

        public UdpClientWrapper()
        {
            _activityCounter = 0;
            _tokenSource = new CancellationTokenSource();
        }

        public UdpClientWrapper(string host, int port)
            : base(new IPEndPoint(IPAddress.Parse(host), port))
        {
            _tokenSource = new CancellationTokenSource();
        }

        public bool IsCanceled => _tokenSource.IsCancellationRequested;

        public async Task<UdpReceiveResult> Receive()
        {
            UdpReceiveResult result;

            try
            {
                _activityCounter++;

                result = await ReceiveAsync();                
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Receive", e);
            }
            finally
            {
                _activityCounter--;
            }

            return result;
        }

        public async new Task<int> Send(byte[] datagram, int bytes, IPEndPoint endPoint)
        {
            int result = 0;

            try
            {
                _activityCounter++;

                result = await SendAsync(datagram, bytes, endPoint);                
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }
            finally
            {
                _activityCounter--;
            }

            return result;
        }

        public async new Task<int> Send(byte[] datagram, int bytes)
        {
            int result = 0;

            try
            {
                _activityCounter++;

                result = await SendAsync(datagram, bytes);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }
            finally
            {
                _activityCounter--;
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client.Disconnect(false);

                int minWaitTime = 10;

                while (_activityCounter > 0)
                {
                    Thread.Sleep(minWaitTime);
                }
            }

            base.Dispose(disposing);
        }

        internal void Abort()
        {
            _tokenSource.Cancel();
        }

        internal void Reset()
        {
            _tokenSource = new CancellationTokenSource();
        }
    }
}

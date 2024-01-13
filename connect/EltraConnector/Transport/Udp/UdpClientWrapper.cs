using EltraCommon.Logger;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EltraCommon.Extensions;

namespace EltraConnector.Transport.Udp
{
    class UdpClientWrapper : IDisposable
    {
        private readonly IUdpClient _udpClient;

        private CancellationTokenSource _tokenSource;
        private int _activityCounter;
        
        public UdpClientWrapper(IUdpClient udpClient)
        {
            _udpClient = udpClient;
            
            _activityCounter = 0;
            _tokenSource = new CancellationTokenSource();
        }

        public UdpClientWrapper(IUdpClient udpClient, string host, int port)
        {
            _udpClient = udpClient;

            _udpClient.Init(host, port);

            _tokenSource = new CancellationTokenSource();
        }

        public bool IsCanceled => _tokenSource.IsCancellationRequested;

        public async Task<UdpReceiveResult> Receive()
        {
            UdpReceiveResult result;

            try
            {
                _activityCounter++;

                result = await _udpClient.ReceiveAsync().WithCancellation(_tokenSource.Token);
            }
            catch(Exception e)
            {
                if(e.GetType() != typeof(OperationCanceledException))
                {
                    MsgLogger.Exception($"{GetType().Name} - Receive", e);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Receive", "close requested");
                }                
            }
            finally
            {
                _activityCounter--;
            }

            return result;
        }

        public async Task<int> Send(byte[] datagram, int bytes, IPEndPoint endPoint)
        {
            int result = 0;

            try
            {
                _activityCounter++;

                result = await _udpClient.SendAsync(datagram, bytes, endPoint).WithCancellation(_tokenSource.Token);
            }
            catch(Exception e)
            {
                if (e.GetType() != typeof(OperationCanceledException))
                {
                    MsgLogger.Exception($"{GetType().Name} - Send", e);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Send", "close requested");
                }
            }
            finally
            {
                _activityCounter--;
            }

            return result;
        }

        public async Task<int> Send(byte[] datagram, int bytes)
        {
            int result = 0;

            try
            {
                _activityCounter++;

                result = await _udpClient.SendAsync(datagram, bytes).WithCancellation(_tokenSource.Token);
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(OperationCanceledException))
                {
                    MsgLogger.Exception($"{GetType().Name} - Send", e);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Send", "close requested");
                }
            }
            finally
            {
                _activityCounter--;
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Abort();

                _udpClient.Disconnect();

                int minWaitTime = 10;
                int maxWaitTime = 3000;

                var sw = new Stopwatch();
                sw.Start();

                while (_activityCounter > 0 && sw.ElapsedMilliseconds < maxWaitTime)
                {
                    Thread.Sleep(minWaitTime);
                }
            }
        }

        internal void Abort()
        {
            _tokenSource.Cancel();
        }

        internal void Reset()
        {
            _tokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Connect(string host, int port)
        {
            _udpClient.Connect(host, port);
        }

        public void Close()
        {
            _udpClient.Close();
        }
    }
}

using EltraCommon.Contracts.Users;
using EltraCommon.Extensions;
using EltraCommon.Logger;
using EltraConnector.Transport.Udp.Contracts;
using EltraConnector.Transport.Udp.Response;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    internal class EltraUdpConnector
    {
        #region Private fields

        private UdpClient _udpClient;        
        private CancellationTokenSource _tokenSource;
        private Encoding _encoding;
        private SocketError _socketErrorCode;

        #endregion

        #region Constructors

        public EltraUdpConnector()
        {
            _tokenSource = new CancellationTokenSource();

            Host = LocalHost;
            Port = 5100;
        }

        #endregion

        #region Properties

        public static string LocalHost => "127.0.0.1";

        public string Host { get; set; }

        public int Port { get; set; }

        protected UdpClient UdpClient => _udpClient ?? (_udpClient = CreateUdpClient());

        protected Encoding Encoding => _encoding ?? (_encoding = new UTF8Encoding());

        public bool IsCanceled => _tokenSource.IsCancellationRequested;

        public SocketError SocketErrorCode 
        {
            get => _socketErrorCode;
            set
            {
                if (_socketErrorCode != value)
                {
                    _socketErrorCode = value;

                    OnSocketErrorCodeChanged(value);
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<SocketError> ErrorRaised;
        public event EventHandler<ReceiveResponse> MessageReceived;

        #endregion

        #region Events handling

        protected void OnMessageReceived(ReceiveResponse e)
        {
            MessageReceived?.Invoke(this, e);
        }

        private void OnSocketErrorCodeChanged(SocketError e)
        {
            if (_socketErrorCode != SocketError.Success)
            {
                ErrorRaised?.Invoke(this, e);
            }
        }

        #endregion

        #region Methods

        protected virtual UdpClient CreateUdpClient()
        {
            var result = new UdpClient(new IPEndPoint(IPAddress.Parse(Host), Port));

            return result;
        }

        public async Task<ReceiveResponse> Receive()
        {
            ReceiveResponse result = null;
                        
            SocketErrorCode = SocketError.Success;

            try
            {
                var receiveResult = await UdpClient.ReceiveAsync().WithCancellation(_tokenSource.Token);

                var txt = Encoding.GetString(receiveResult.Buffer, 0, receiveResult.Buffer.Length);

                result = new ReceiveResponse() { Endpoint = receiveResult.RemoteEndPoint, Text = txt };
            }
            catch (SocketException e)
            {
                SocketErrorCode = e.SocketErrorCode;
            }
            catch (ObjectDisposedException)
            {
                SocketErrorCode = SocketError.NotInitialized;
            }
            catch (Exception)
            {
                SocketErrorCode = SocketError.OperationAborted;
            }

            return result;
        }

        public async Task<int> Send<T>(IPEndPoint endPoint, UserIdentity identity, T obj)
        {
            int result = 0;

            try
            {
                var msg = JsonConvert.SerializeObject(obj);

                result = await Send(endPoint, identity, typeof(T).FullName, msg);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }

            return result;
        }

        private async Task<int> Send(IPEndPoint endPoint, UserIdentity identity, string className, string msg)
        {
            int bytesSent = -1;
            var request = new UdpRequest() { Identity = identity, TypeName = className, Data = msg };

            try
            {
                bytesSent = await Send(endPoint, JsonConvert.SerializeObject(request));
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }

            return bytesSent;
        }

        public async Task<int> Send(IPEndPoint endPoint, string data)
        {
            int result = 0;
                        
            SocketErrorCode = SocketError.Success;

            try
            {
                var bytes = Encoding.GetBytes(data);

                result = await UdpClient.SendAsync(bytes, bytes.Length, endPoint).WithCancellation(_tokenSource.Token);
            }
            catch (SocketException e)
            {                
                SocketErrorCode = e.SocketErrorCode;
            }
            catch (ObjectDisposedException)
            {
                SocketErrorCode = SocketError.NotInitialized;
            }
            catch (Exception)
            {
                SocketErrorCode = SocketError.OperationAborted;
            }

            return result;
        }

        protected async Task<int> Send(byte[] bytes, int length)
        {
            int result = 0;
                        
            SocketErrorCode = SocketError.Success;

            try
            {
                result = await UdpClient.SendAsync(bytes, length).WithCancellation(_tokenSource.Token);                
            }
            catch (SocketException e)
            {
                SocketErrorCode = e.SocketErrorCode;
            }
            catch (ObjectDisposedException)
            {
                SocketErrorCode = SocketError.NotInitialized;
            }
            catch (Exception)
            {
                SocketErrorCode = SocketError.OperationAborted;
            }

            return result;
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
        }

        #endregion
    }
}

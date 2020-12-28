using EltraCommon.Contracts.Users;
using EltraCommon.Logger;
using EltraConnector.Transport.Udp.Contracts;
using EltraConnector.Transport.Udp.Response;
using System.Text.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EltraCommon.Extensions;
using EltraCommon.Helpers;
using EltraConnector.Extensions;

namespace EltraConnector.Transport.Udp
{
    internal class EltraUdpConnector : IDisposable
    {
        #region Private fields

        private UdpClientWrapper _udpClient;        
        
        private Encoding _encoding;
        private SocketError _socketErrorCode;
        private bool disposedValue;

        #endregion

        #region Constructors

        public EltraUdpConnector()
        {
            Host = LocalHost;
            Port = 5100;
        }

        #endregion

        #region Properties

        public static string LocalHost => "127.0.0.1";

        public string Host { get; set; }

        public int Port { get; set; }

        protected UdpClientWrapper UdpClient => _udpClient ?? (_udpClient = CreateUdpClient());

        protected Encoding Encoding => _encoding ?? (_encoding = new UTF8Encoding());

        public bool IsCanceled => UdpClient.IsCanceled;

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
        public event EventHandler<int> MessageSent;

        #endregion

        #region Events handling

        protected void OnMessageReceived(ReceiveResponse e)
        {
            MessageReceived?.Invoke(this, e);
        }
        protected void OnMessageSent(int length)
        {
            MessageSent?.Invoke(this, length);
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

        protected virtual UdpClientWrapper CreateUdpClient()
        {
            var result = new UdpClientWrapper(Host, Port);

            return result;
        }

        public async Task<ReceiveResponse> Receive()
        {
            ReceiveResponse result = null;
                        
            SocketErrorCode = SocketError.Success;

            try
            {
                var receiveResult = await UdpClient.Receive();

                if (receiveResult != null && receiveResult.Buffer != null)
                {
                    var txt = Encoding.GetString(receiveResult.Buffer, 0, receiveResult.Buffer.Length);

                    result = new ReceiveResponse() { Endpoint = receiveResult.RemoteEndPoint, Text = txt };
                }
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
                var msg = JsonSerializer.Serialize(obj);

                result = await Send(endPoint, identity, typeof(T).FullName, msg);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }

            return result;
        }

        public async Task<int> Send(IPEndPoint endPoint, UserIdentity identity, string className, string msg)
        {
            int bytesSent = -1;
            var data = msg.ToBase64();
            
            var request = new UdpRequest() { Identity = identity.HashPassword(), TypeName = className, Data = data, Checksum = CryptHelpers.ToMD5(data) };

            try
            {
                bytesSent = await Send(endPoint, JsonSerializer.Serialize(request));
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

                result = await UdpClient.Send(bytes, bytes.Length, endPoint);

                OnMessageSent(bytes.Length);
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
                result = await UdpClient.Send(bytes, length);

                OnMessageSent(length);
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

        public void Abort()
        {
            UdpClient.Abort();
        }

        #endregion

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Abort();

                    UdpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

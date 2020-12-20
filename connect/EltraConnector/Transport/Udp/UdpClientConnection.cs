using EltraCommon.Contracts.Users;
using EltraConnector.Transport.Definitions;
using EltraConnector.Transport.Events;
using EltraConnector.Transport.Ws.Interfaces;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    class UdpClientConnection : IConnection
    {
        #region Private fields

        private string _uniqueId;
        private EltraUdpClient _client;
        private string _url;

        #endregion

        #region Properties

        public string Url 
        { 
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnUrlChanged();
                }
            }
        }
        
        public string UniqueId { get => _uniqueId ?? (_uniqueId = Guid.NewGuid().ToString()); set => _uniqueId = value; }
        public string ChannelName { get; set; }
        public bool IsConnected { get; set; }
        public bool IsDisconnecting { get; set; }
        public bool Fallback => true;
        protected EltraUdpClient Client { get => _client ?? (_client = CreateClient()); }
        public ConnectionPriority Priority => ConnectionPriority.High;

        public int ProcessedRequestsCount { get; internal set; }

        #endregion

        #region Events

        public event EventHandler<ConnectionMessageEventArgs> MessageReceived;
        public event EventHandler<ConnectionMessageEventArgs> MessageSent;
        public event EventHandler<ConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Events handling

        private void OnUrlChanged()
        {
            if(IsConnected)
            {
                Disconnect();
            }
        }

        private void OnClientErrorRaised(object sender, SocketError e)
        {
            ErrorOccured?.Invoke(this, new ConnectionMessageEventArgs() { Source = UniqueId, Message = $"udp socket error, error code = {e}", Type = MessageType.Text });
        }

        private void OnClientMessageReceived(object sender, Response.ReceiveResponse e)
        {
            if (MessageReceived != null)
            {
                MessageReceived.Invoke(this, new ConnectionMessageEventArgs() { Source = e.Endpoint.ToString(), Message = e.Text, Type = MessageType.Text });

                e.Handled = true;
            }
        }

        private void OnClientMessageSent(object sender, int bytesSent)
        {
            MessageSent?.Invoke(this, new ConnectionMessageEventArgs() { Source = UniqueId, Message = $"{bytesSent}", Type = MessageType.Text });
        }

        #endregion

        #region Methods

        private EltraUdpClient CreateClient()
        {
            var result = new EltraUdpClient() { Url = Url };

            result.MessageReceived += OnClientMessageReceived;
            result.MessageSent += OnClientMessageSent;
            result.ErrorRaised += OnClientErrorRaised;

            return result;
        }

        public Task<bool> Connect()
        {
            bool result = Client.Connect();

            IsConnected = result;

            return Task.FromResult(result);
        }

        public Task<bool> Disconnect()
        {
            IsDisconnecting = true;

            bool result = Client.Disconnect();

            if(result)
            {
                IsConnected = false;
            }

            IsDisconnecting = false;

            return Task.FromResult(result);
        }

        public Task<string> Receive()
        {
            return Task.FromResult(string.Empty);
        }

        public Task<T> Receive<T>()
        {
            return (Task<T>)Task.Run(() => { return null; });
        }

        public async Task<bool> Send(UserIdentity identity, string typeName, string data)
        {
            var sentResult = await Client.Send(identity, typeName, data);

            return sentResult > 0;
        }

        public async Task<bool> Send<T>(UserIdentity identity, T obj)
        {
            var sentResult = await Client.Send(identity, obj);

            return sentResult > 0;
        }

        #endregion
    }
}

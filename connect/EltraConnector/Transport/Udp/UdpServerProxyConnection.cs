using EltraCommon.Contracts.Users;
using EltraConnector.Transport.Definitions;
using EltraConnector.Transport.Events;
using EltraConnector.Transport.Ws.Interfaces;
using System;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    class UdpServerProxyConnection : IConnection
    {
        #region Private fields

        private UdpServerConnection _source;

        #endregion

        #region Constructors

        public UdpServerProxyConnection(UdpServerConnection source)
        {
            _source = source;
        }

        #endregion

        #region Properties

        public string Url { get; set; }
        public string UniqueId { get; set; }
        public string ChannelName { get; set; }

        public bool IsConnected => _source.IsConnected;

        public bool IsDisconnecting => _source.IsDisconnecting;

        public bool Fallback => _source.Fallback;

        public ConnectionPriority Priority => _source.Priority;

        public bool ReceiveSupported => _source.ReceiveSupported;

        #endregion

        #region Events

        public event EventHandler<ConnectionMessageEventArgs> MessageReceived;
        public event EventHandler<ConnectionMessageEventArgs> MessageSent;
        public event EventHandler<ConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Events handling

        protected void OnMessageReceived()
        {
            MessageReceived?.Invoke(this, new ConnectionMessageEventArgs());
        }

        protected void OnMessageSent()
        {
            MessageSent?.Invoke(this, new ConnectionMessageEventArgs());
        }

        protected void OnErrorOccured()
        {
            ErrorOccured?.Invoke(this, new ConnectionMessageEventArgs());
        }

        #endregion

        #region Methods

        public Task<bool> Connect()
        {
            return Task.FromResult(true);
        }

        public Task<bool> Disconnect()
        {
            return Task.FromResult(true);
        }

        public Task<string> Receive()
        {
            return _source.Receive();
        }

        public Task<T> Receive<T>()
        {
            return _source.Receive<T>();
        }

        public async Task<bool> Send(UserIdentity identity, string typeName, string data)
        {
            bool result = false;

            if(_source != null)
            {
                result = await _source.Send(identity, typeName, data);
            }

            return result;
        }

        public async Task<bool> Send<T>(UserIdentity identity, T obj)
        {
            bool result = false;

            if (_source != null)
            {
                result = await _source.Send(identity, obj);
            }

            return result;
        }

        #endregion
    }
}

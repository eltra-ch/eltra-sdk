using EltraCommon.Contracts.Users;
using EltraConnector.Helpers;
using EltraConnector.Transport.Definitions;
using EltraConnector.Transport.Events;
using EltraConnector.Transport.Udp.Contracts;
using EltraConnector.Transport.Udp.Response;
using EltraConnector.Transport.Ws.Interfaces;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;
using EltraCommon.Logger;
using System.Net;
using EltraCommon.Extensions;

namespace EltraConnector.Transport.Udp
{
    class UdpServerConnection : IConnection
    {
        #region Private fields

        private bool _isConnected;
        private EltraUdpServer _server;
        private EndpointStore _endpointStore;

        #endregion

        #region Properties

        public string Url { get; set; }
        public string UniqueId { get; set; }
        public string ChannelName { get; set; }
        public bool IsConnected 
        {
            get
            {
                _isConnected = Server.IsRunning;

                return _isConnected;
            }
            set
            {
                _isConnected = value;
            } 
        }
        public bool IsDisconnecting { get; set; }
        public bool Fallback => true;
        protected EltraUdpServer Server => _server ?? (_server = CreateServer());
        protected EndpointStore EndpointStore => _endpointStore ?? (_endpointStore = new EndpointStore());

        public ConnectionPriority Priority => ConnectionPriority.High;

        public string LocalHost => $"{IpHelper.GetLocalIpAddress()}:{Server.Port}";

        public bool ReceiveSupported => false;

        #endregion

        #region Events

        public event EventHandler<ConnectionMessageEventArgs> MessageReceived;
        public event EventHandler<ConnectionMessageEventArgs> MessageSent;
        public event EventHandler<ConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Events handling

        private void OnServerMessageReceived(object sender, ReceiveResponse e)
        {
            if (MessageReceived != null)
            {
                UpdateEndpointStore(e.Text, e.Endpoint);

                MessageReceived.Invoke(this, new ConnectionMessageEventArgs() { Source = e.Endpoint.ToString(), Message = e.Text, Type = MessageType.Text });

                e.Handled = true;
            }
        }

        private void UpdateEndpointStore(string message, IPEndPoint endPoint)
        {
            try
            {
                var udpRequest = message.TryDeserializeObject<UdpRequest>();

                if (udpRequest is UdpRequest)
                {
                    _endpointStore.Add(udpRequest.Identity, endPoint);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - OnServerMessageReceived", e);
            }
        }

        private void OnServerMessageSent(object sender, int bytesSent)
        {
            MessageSent?.Invoke(this, new ConnectionMessageEventArgs() { Source = UniqueId, Message = $"{bytesSent}", Type = MessageType.Text });
        }

        private void OnServerErrorRaised(object sender, SocketError e)
        {
            ErrorOccured?.Invoke(this, new ConnectionMessageEventArgs() { Source = UniqueId, Message = $"udp socket error, error code = {e}", Type = MessageType.Text });
        }

        #endregion

        #region Methods

        protected virtual EltraUdpServer CreateServer()
        {
            var rnd = new Random();

            var result = new EltraUdpServer() { Host = "0.0.0.0", Port = rnd.Next(5100, 5199) };

            result.ErrorRaised += OnServerErrorRaised;
            result.MessageSent += OnServerMessageSent;
            result.MessageReceived += OnServerMessageReceived;

            return result;
        }

        public Task<bool> Connect()
        {
            bool result = false;

            if (!Server.IsRunning)
            {
                result = Server.Start();
            }

            return Task.FromResult(result);
        }

        public Task<bool> Disconnect()
        {
            bool result = false;

            if(Server.IsRunning)
            {
                result = Server.Stop();
            }

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
            bool result = false;
            var endPoints = EndpointStore.ActiveEndpoints;

            foreach(var endpoint in endPoints)
            {
                var sentResult = await Server.Send(endpoint, identity, typeName, data);

                result = sentResult > 0;
            }

            return result;
        }

        public async Task<bool> Send<T>(UserIdentity identity, T obj)
        {
            bool result = false;
            var endPoints = EndpointStore.ActiveEndpoints;

            foreach (var endpoint in endPoints)
            {
                var sentResult = await Server.Send(endpoint, identity, obj);
             
                result = sentResult > 0;
            }

            return result;
        }

        #endregion
    }
}

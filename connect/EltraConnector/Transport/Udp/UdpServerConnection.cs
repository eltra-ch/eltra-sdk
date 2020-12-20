﻿using EltraCommon.Contracts.Users;
using EltraConnector.Helpers;
using EltraConnector.Transport.Definitions;
using EltraConnector.Transport.Events;
using EltraConnector.Transport.Udp.Response;
using EltraConnector.Transport.Ws.Interfaces;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    class UdpServerConnection : IConnection
    {
        #region Private fields

        private EltraUdpServer _server;
        private EndpointStore _endpointStore;

        #endregion

        #region Properties

        public string Url { get; set; }
        public string UniqueId { get; set; }
        public string ChannelName { get; set; }
        public bool IsConnected { get; set; }
        public bool IsDisconnecting { get; set; }
        public bool Fallback => true;
        protected EltraUdpServer Server => _server ?? (_server = CreateServer());
        protected EndpointStore EndpointStore => _endpointStore ?? (_endpointStore = new EndpointStore());

        public ConnectionPriority Priority => ConnectionPriority.High;

        public string LocalHost => $"{IpHelper.GetLocalIpAddress()}:{Server.Port}";

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
                MessageReceived.Invoke(this, new ConnectionMessageEventArgs() { Source = e.Endpoint.ToString(), Message = e.Text, Type = MessageType.Text });

                e.Handled = true;
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
            var endPoint = EndpointStore.Lookup(identity);

            if (endPoint != null)
            {
                var sentResult = await Server.Send(endPoint, identity, typeName, data);
                result = sentResult > 0;
            }

            return result;
        }

        public async Task<bool> Send<T>(UserIdentity identity, T obj)
        {
            bool result = false;
            var endPoint = EndpointStore.Lookup(identity);

            if (endPoint != null)
            {
                var sentResult = await Server.Send(endPoint, identity, obj);
                result = sentResult > 0;
            }

            return result;
        }

        #endregion
    }
}

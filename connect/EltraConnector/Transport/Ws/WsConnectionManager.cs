using EltraConnector.Transport.Ws.Converters;
using EltraConnector.Transport.Ws.Interfaces;
using EltraCommon.Contracts.Users;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using EltraConnector.Transport.Ws.Events;

namespace EltraConnector.Transport.Ws
{
    /// <summary>
    /// WsConnectionManager
    /// </summary>
    public class WsConnectionManager : IConnectionManager
    {
        #region Private fields

        private List<WsConnection> _connectionList;

        #endregion

        #region Constructors

        /// <summary>
        /// WsConnectionManager
        /// </summary>
        public WsConnectionManager()
        {
            _connectionList = new List<WsConnection>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// HostUrl
        /// </summary>
        public string HostUrl { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// MessageReceived
        /// </summary>
        public event EventHandler<WsConnectionMessageEventArgs> MessageReceived;
        /// <summary>
        /// MessageSent
        /// </summary>
        public event EventHandler<WsConnectionMessageEventArgs> MessageSent;
        /// <summary>
        /// ErrorOccured
        /// </summary>
        public event EventHandler<WsConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Events handling

        private void OnConnectionMessageReceived(object source, WsConnectionMessageEventArgs e)
        {
            MessageReceived?.Invoke(source, e);
        }
        private void OnConnectionMessageSent(object source, WsConnectionMessageEventArgs e)
        {
            MessageSent?.Invoke(source, e);
        }

        private void OnConnectionErrorOccured(object source, WsConnectionMessageEventArgs e)
        {
            ErrorOccured?.Invoke(source, e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public async Task<bool> Connect(string uniqueId, string channelName)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if(connection == null)
            {
                connection = new WsConnection(uniqueId, channelName);

                if(await connection.Connect(WsHostUrlConverter.ToWsUrl(HostUrl)))
                {
                    RegisterConnectionEvents(connection);

                    _connectionList.Add(connection);
                    result = true;
                }
            }
            else if(!connection.IsConnected)
            {
                if (await connection.Connect(WsHostUrlConverter.ToWsUrl(HostUrl)))
                {                    
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// IsConnected
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public bool IsConnected(string uniqueId)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if(connection!=null)
            {
                result = connection.IsConnected;
            }

            return result;
        }

        /// <summary>
        /// CanConnect
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public bool CanConnect(string uniqueId)
        {
            bool result = true;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = !(connection.IsConnected || connection.IsDisconnecting);
            }

            return result;
        }

        /// <summary>
        /// IsDisconnecting
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public bool IsDisconnecting(string uniqueId)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = connection.IsDisconnecting;
            }

            return result;
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public async Task<bool> Disconnect(string uniqueId)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = await connection.Disconnect();

                if (result)
                {
                    UnregisterConnectionEvents(connection);

                    _connectionList.Remove(connection);
                }
            }

            return result;
        }

        /// <summary>
        /// DisconnectAll
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DisconnectAll()
        {
            bool result = true;

            var activeConnections = _connectionList.ToArray();

            foreach (var connection in activeConnections)
            {
                result = await connection.Disconnect();

                if(!result)
                {
                    break;
                }
                else
                {
                    UnregisterConnectionEvents(connection);

                    _connectionList.Remove(connection);
                }
            }

            return result;
        }

        private void RegisterConnectionEvents(WsConnection connection)
        {
            if (connection != null)
            {
                connection.MessageReceived += OnConnectionMessageReceived;
                connection.MessageSent += OnConnectionMessageSent;
                connection.ErrorOccured += OnConnectionErrorOccured;
            }
        }

        private void UnregisterConnectionEvents(WsConnection connection)
        {
            if (connection != null)
            {
                connection.MessageReceived -= OnConnectionMessageReceived;
                connection.MessageSent -= OnConnectionMessageSent;
                connection.ErrorOccured -= OnConnectionErrorOccured;
            }
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueId"></param>
        /// <param name="identity"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<bool> Send<T>(string uniqueId, UserIdentity identity, T obj)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = await connection.Send(identity, obj);
            }

            return result;
        }

        /// <summary>
        /// Receive
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public async Task<string> Receive(string uniqueId)
        {
            string result = string.Empty;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = await connection.Receive();
            }

            return result;
        }

        /// <summary>
        /// Receive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public async Task<T> Receive<T>(string uniqueId)
        {
            T result = default;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = await connection.Receive<T>();
            }

            return result;
        }

        private WsConnection FindConnection(string uniqueId)
        {
            WsConnection result = null;

            foreach(var connection in _connectionList)
            {
                if(connection.UniqueId == uniqueId)
                {
                    result = connection;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="identity"></param>
        /// <param name="typeName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected async Task<bool> Send(string uniqueId, UserIdentity identity, string typeName, string data)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = await connection.Send(identity, typeName, data);
            }

            return result;
        }

        #endregion
    }
}

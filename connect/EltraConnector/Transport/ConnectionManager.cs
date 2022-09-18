using EltraConnector.Transport.Ws.Interfaces;
using EltraCommon.Contracts.Users;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;
using EltraConnector.Transport.Events;
using EltraConnector.Transport.Factory;
using EltraCommon.Logger;
using System.Linq;

namespace EltraConnector.Transport
{
    /// <summary>
    /// ConnectionManager
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        #region Private fields

        private readonly SemaphoreSlim _connectionLock;
        private readonly SemaphoreSlim _sendLock;
        private readonly List<object> _clientList;

        private List<IConnection> _connectionList;
        
        #endregion

        #region Constructors

        /// <summary>
        /// ConnectionManager
        /// </summary>
        public ConnectionManager()
        {
            _clientList = new List<object>();
            _connectionLock = new SemaphoreSlim(1);
            _sendLock = new SemaphoreSlim(1);
            _connectionList = new List<IConnection>();
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
        public event EventHandler<ConnectionMessageEventArgs> MessageReceived;
        /// <summary>
        /// MessageSent
        /// </summary>
        public event EventHandler<ConnectionMessageEventArgs> MessageSent;
        /// <summary>
        /// ErrorOccured
        /// </summary>
        public event EventHandler<ConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Events handling

        private void OnConnectionMessageReceived(object source, ConnectionMessageEventArgs e)
        {
            MessageReceived?.Invoke(source, e);
        }
        private void OnConnectionMessageSent(object source, ConnectionMessageEventArgs e)
        {
            MessageSent?.Invoke(source, e);
        }

        private void OnConnectionErrorOccured(object source, ConnectionMessageEventArgs e)
        {
            ErrorOccured?.Invoke(source, e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// GetConnection
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public IConnection GetConnection<T>(string uniqueId)
        {
            IConnection result = null;
            var connections = FindConnections(uniqueId);

            foreach(var connection in connections)
            {
                if(connection is T)
                {
                    result = connection;
                    break;
                }
            }

            return result;
        }

        private bool ClientExists(object client)
        {
            bool existingClient = false;

            foreach (var c in _clientList)
            {
                if (c == client)
                {
                    existingClient = true;
                    break;
                }
            }

            return existingClient;
        }

        /// <summary>
        /// CanDisconnect
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool CanDisconnect(object client)
        {
            bool result = false;

            _connectionLock.Wait();

            if(_clientList.Count == 0)
            {
                result = true;
            }
            else if(_clientList.Count == 1)
            {
                if(_clientList[0] == client)
                {
                    result = true;
                }
            }

            _connectionLock.Release();

            return result;
        }

        /// <summary>
        /// RegisterChannelClient
        /// </summary>
        /// <param name="client"></param>
        public void RegisterChannelClient(object client)
        {
            _connectionLock.Wait();

            if (!ClientExists(client))
            {
                _clientList.Add(client);
            }

            _connectionLock.Release();
        }

        /// <summary>
        /// UnregisterChannelClient
        /// </summary>
        /// <param name="client"></param>
        public void UnregisterChannelClient(object client)
        {
            _connectionLock.Wait();

            if (ClientExists(client))
            {
                _clientList.Remove(client);
            }

            _connectionLock.Release();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public async Task<bool> Connect(string uniqueId, string channelName)
        {
            bool result = false;
            var connections = FindConnections(uniqueId);

            if(connections.Count == 0)
            {
                connections = ConnectionFactory.CreateConnections(uniqueId, channelName, HostUrl);
                
                result = await Connect(connections);
            }
            else
            {
                foreach (var connection in connections)
                {
                    if (!connection.IsConnected)
                    {
                        if (await connection.Connect())
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        private async Task<bool> Connect(List<IConnection> connections)
        {
            bool result = false;

            foreach (var connection in connections)
            {
                result = await Connect(connection);
            }

            return result;
        }

        private bool ConnectionExists(IConnection connection)
        {
            bool result = false;
            
            foreach (var c in _connectionList)
            {
                if (c.UniqueId == connection.UniqueId && 
                    c.Url == connection.Url &&
                    c.GetType() == connection.GetType())
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<bool> Connect(IConnection connection)
        {
            bool result = false;

            if (await connection.Connect())
            {
                RegisterConnectionEvents(connection);

                await _connectionLock.WaitAsync();

                if (!ConnectionExists(connection))
                {
                    _connectionList.Add(connection);
                }
                else
                {
                    UnregisterConnectionEvents(connection);

                    await connection.Disconnect();
                }

                _connectionLock.Release();

                result = true;
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
            var connections = FindConnections(uniqueId);

            if (connections.Count > 0)
            {
                foreach (var connection in connections)
                {
                    result = true;

                    if (!connection.IsConnected)
                    {
                        result = false;
                        break;
                    }
                }
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
            bool result = false;
            var connections = FindConnections(uniqueId);

            if (connections.Count > 0)
            {
                foreach (var connection in connections)
                {
                    result = !(connection.IsConnected || connection.IsDisconnecting);

                    if (result)
                    {
                        break;
                    }
                }
            }
            else
            {
                result = true;
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
            var connections = FindConnections(uniqueId);

            foreach (var connection in connections)
            {
                result = connection.IsDisconnecting;

                if(result)
                {
                    break;
                }
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
            var connections = FindConnections(uniqueId);

            foreach (var connection in connections)
            {
                result = await connection.Disconnect();

                if (result)
                {
                    result = Remove(uniqueId);
                }
            }

            return result;
        }

        /// <summary>
        /// Remove connection from pool
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public bool Remove(string uniqueId)
        {
            bool result = false;
            var connections = FindConnections(uniqueId);

            if (connections.Count > 0)
            {
                foreach (var connection in connections)
                {
                    UnregisterConnectionEvents(connection);

                    _connectionLock.Wait();

                    _connectionList.Remove(connection);

                    _connectionLock.Release();
                }

                result = true;
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

            await _connectionLock.WaitAsync();

            var activeConnections = _connectionList.ToArray();

            _connectionLock.Release();

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

                    await _connectionLock.WaitAsync();

                    _connectionList.Remove(connection);

                    _connectionLock.Release();
                }
            }

            return result;
        }

        private void RegisterConnectionEvents(IConnection connection)
        {
            if (connection != null)
            {
                connection.MessageReceived += OnConnectionMessageReceived;
                connection.MessageSent += OnConnectionMessageSent;
                connection.ErrorOccured += OnConnectionErrorOccured;
            }
        }

        private void UnregisterConnectionEvents(IConnection connection)
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
            var connections = FindConnections(uniqueId);

            foreach (var connection in connections)
            {
                if(await connection.Send(identity, obj))
                {
                    result = true;

                    if(!connection.Fallback)
                    {
                        break;
                    }
                }
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
            var connections = FindConnections(uniqueId);

            foreach (var connection in connections)
            {
                try
                {
                    var receiveResult = await connection.Receive();

                    if (!string.IsNullOrEmpty(receiveResult))
                    {
                        result = receiveResult;
                        break;
                    }
                }
                catch(Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Receive", e);
                }                
            }

            return result;
        }

        /// <summary>
        /// Receive
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="shouldRunFunc"></param>
        /// <returns></returns>
        public async Task<string> Receive(string uniqueId, Func<bool> shouldRunFunc)
        {
            const int executeIntervalWs = 1;

            string result = string.Empty;

            try
            {
                string msg = string.Empty;

                do
                {
                    msg = await Receive(uniqueId);

                    if (shouldRunFunc())
                    {
                        await Task.Delay(executeIntervalWs);
                    }
                }
                while ((msg == "KEEPALIVE" || msg == "ACK" || string.IsNullOrEmpty(msg)) && shouldRunFunc());

                result = msg;
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Receive", e);
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
            var connections = FindConnections(uniqueId);

            foreach (var connection in connections)
            {
                if (connection.ReceiveSupported)
                {
                    try
                    {
                        var receiveResult = await connection.Receive<T>();

                        if (receiveResult != null)
                        {
                            result = receiveResult;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        MsgLogger.Exception($"{GetType().Name} - Receive", e);
                    }
                }
            }

            return result;
        }

        private List<IConnection> FindConnections(string uniqueId)
        {
            var result = new List<IConnection>();

            _connectionLock.Wait();

            foreach (var connection in _connectionList)
            {
                if(connection.UniqueId == uniqueId)
                {
                    result.Add(connection);
                }
            }

            _connectionLock.Release();

            result = result.OrderByDescending(o => o.Priority).ToList();

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
            var connections = FindConnections(uniqueId);

            foreach (var connection in connections)
            {
                if(await connection.Send(identity, typeName, data))
                {
                    result = true;
                    
                    if (!connection.Fallback)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Lock
        /// </summary>
        public async Task Lock()
        {
            await _sendLock.WaitAsync();
        }

        /// <summary>
        /// Unlock
        /// </summary>
        public void Unlock()
        {
            _sendLock.Release();
        }

        #endregion
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace EltraConnector.Ws
{
    class WsConnectionManager
    {
        #region Private fields

        private List<WsConnection> _connectionList;

        #endregion

        #region Properties

        public string HostUrl { get; set; }

        #endregion

        #region Constructors

        public WsConnectionManager()
        {
            _connectionList = new List<WsConnection>();
        }

        #endregion

        #region Methods

        public async Task<bool> Connect(string uniqueId, string channelName)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if(connection == null)
            {
                connection = new WsConnection(uniqueId, channelName);

                if(await connection.Connect(WsHostUrlConverter.ToWsUrl(HostUrl)))
                {
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

        public async Task<bool> Disconnect(string uniqueId)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = await connection.Disconnect();

                if (result)
                {
                    _connectionList.Remove(connection);
                }
            }

            return result;
        }

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
                    _connectionList.Remove(connection);
                }
            }

            return result;
        }

        private async Task<bool> Send(string uniqueId, string typeName, string data)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = await connection.Send(typeName, data);
            }

            return result;
        }

        public async Task<bool> Send<T>(string uniqueId, T obj)
        {
            bool result = false;
            var connection = FindConnection(uniqueId);

            if (connection != null)
            {
                result = await connection.Send(obj);
            }

            return result;
        }

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

        #endregion
    }
}

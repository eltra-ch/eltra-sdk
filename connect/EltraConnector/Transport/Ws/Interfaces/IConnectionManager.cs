using EltraCommon.Contracts.Users;
using EltraConnector.Transport.Ws.Events;
using System;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Ws.Interfaces
{
    /// <summary>
    /// Connection manager
    /// </summary>
    public interface IConnectionManager
    {
        #region Events

        /// <summary>
        /// MessageReceived
        /// </summary>
        event EventHandler<WsConnectionMessageEventArgs> MessageReceived;
        /// <summary>
        /// MessageSent
        /// </summary>
        event EventHandler<WsConnectionMessageEventArgs> MessageSent;
        /// <summary>
        /// ErrorOccured
        /// </summary>
        event EventHandler<WsConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Methods

        /// <summary>
        /// Connect to channel
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="channelName"></param>
        /// <returns></returns>
        Task<bool> Connect(string uniqueId, string channelName);
        /// <summary>
        /// IsConnected
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        bool IsConnected(string uniqueId);
        /// <summary>
        /// CanConnect
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        bool CanConnect(string uniqueId);
        /// <summary>
        /// IsDisconnecting
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        bool IsDisconnecting(string uniqueId);
        /// <summary>
        /// Disconnect
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        Task<bool> Disconnect(string uniqueId);
        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        bool Remove(string uniqueId);
        /// <summary>
        /// DisconnectAll
        /// </summary>
        /// <returns></returns>
        Task<bool> DisconnectAll();
        /// <summary>
        /// Send
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueId"></param>
        /// <param name="identity"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<bool> Send<T>(string uniqueId, UserIdentity identity, T obj);
        /// <summary>
        /// Receive
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        Task<string> Receive(string uniqueId);
        /// <summary>
        /// Receive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        Task<T> Receive<T>(string uniqueId);

        #endregion
    }
}

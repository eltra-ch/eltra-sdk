﻿using EltraCommon.Contracts.Users;
using EltraConnector.Transport.Events;
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
        event EventHandler<ConnectionMessageEventArgs> MessageReceived;
        /// <summary>
        /// MessageSent
        /// </summary>
        event EventHandler<ConnectionMessageEventArgs> MessageSent;
        /// <summary>
        /// ErrorOccured
        /// </summary>
        event EventHandler<ConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Methods

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<bool> Connect(IConnection connection);

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
        /// Get specific connection
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        IConnection GetConnection<T>(string uniqueId);
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
        /// <param name="uniqueId"></param>
        /// <param name="shouldRunFunc"></param>
        /// <returns></returns>
        Task<string> Receive(string uniqueId, Func<bool> shouldRunFunc);

        /// <summary>
        /// Receive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        Task<T> Receive<T>(string uniqueId);

        /// <summary>
        /// Lock
        /// </summary>
        Task Lock();

        /// <summary>
        /// Unlock
        /// </summary>
        void Unlock();

        /// <summary>
        /// RegisterChannelClient
        /// </summary>
        /// <param name="client"></param>
        void RegisterChannelClient(object client);

        /// <summary>
        /// UnregisterChannelClient
        /// </summary>
        /// <param name="client"></param>
        void UnregisterChannelClient(object client);

        /// <summary>
        /// CanDisconnect
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        bool CanDisconnect(object client);

        #endregion
    }
}

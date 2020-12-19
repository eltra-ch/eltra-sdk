using EltraCommon.Contracts.Users;
using EltraConnector.Transport.Definitions;
using EltraConnector.Transport.Events;
using System;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Ws.Interfaces
{
    /// <summary>
    /// IConnection
    /// </summary>
    public interface IConnection
    {
        #region Properties

        /// <summary>
        /// url
        /// </summary>
        string Url { get; set; }
        /// <summary>
        /// unique id
        /// </summary>
        string UniqueId { get; set; }
        /// <summary>
        /// channel name
        /// </summary>
        string ChannelName { get; set; }
        /// <summary>
        /// is connected
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// is disconnecting now
        /// </summary>
        bool IsDisconnecting { get; }

        /// <summary>
        /// Fallback
        /// </summary>
        bool Fallback { get; }

        /// <summary>
        /// Priority
        /// </summary>
        ConnectionPriority Priority { get; }

        #endregion

        #region Events

        /// <summary>
        /// Message received
        /// </summary>
        event EventHandler<ConnectionMessageEventArgs> MessageReceived;
        /// <summary>
        /// Message sent
        /// </summary>
        event EventHandler<ConnectionMessageEventArgs> MessageSent;
        /// <summary>
        /// Error occured
        /// </summary>
        event EventHandler<ConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Methods

        /// <summary>
        /// Connect
        /// </summary>
        /// <returns></returns>
        Task<bool> Connect();

        /// <summary>
        /// Disconnect
        /// </summary>
        /// <returns></returns>
        Task<bool> Disconnect();

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="typeName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> Send(UserIdentity identity, string typeName, string data);

        /// <summary>
        /// Send
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<bool> Send<T>(UserIdentity identity, T obj);

        /// <summary>
        /// Receive
        /// </summary>
        /// <returns></returns>
        Task<string> Receive();

        /// <summary>
        /// Receive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Receive<T>();

        #endregion
    }
}

using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraConnector.Agent.Events;
using EltraConnector.Agent.UserAgent.Events;
using EltraConnector.UserAgent.Definitions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EltraConnector.Interfaces
{
    /// <summary>
    /// IAgentConnector - agent connector interface
    /// </summary>
    public interface IAgentConnector
    {
        #region Properties

        /// <summary>
        /// ELTRA Cloud IoT Service host name
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// Agent authorisation data 
        /// </summary>
        UserIdentity Identity { get; }

        /// <summary>
        /// Channel created by agent
        /// </summary>
        Channel Channel { get; }

        /// <summary>
        /// Agent status
        /// </summary>
        AgentStatus Status { get; }

        /// <summary>
        /// Timeout in seconds (channel timeout, connection timeout)
        /// </summary>
        uint Timeout { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Agent status changed
        /// </summary>
        event EventHandler<AgentStatusEventArgs> StatusChanged;

        /// <summary>
        /// Device node detected
        /// </summary>
        event EventHandler<DeviceDetectedEventArgs> DeviceDetected;

        #endregion

        #region Methods

        /// <summary>
        /// Connect to host
        /// </summary>
        /// <returns></returns>
        Task<bool> Connect();

        /// <summary>
        /// Connect to host and bind agent channel with master device channel
        /// </summary>
        /// <returns></returns>
        Task<bool> Connect(UserIdentity deviceIdentity);

        /// <summary>
        /// Disconnect
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sign in user
        /// </summary>
        /// <param name="identity">User identity</param>
        /// <param name="createAccount"></param>
        /// <returns>true on success</returns>
        Task<bool> SignIn(UserIdentity identity, bool createAccount = false);

        /// <summary>
        /// Sign out agent user
        /// </summary>
        /// <returns>true on success</returns>
        Task<bool> SignOut();

        /// <summary>
        /// Sign-off - removes the account.
        /// </summary>
        /// <returns>true of success</returns>
        Task<bool> SignOff();

        /// <summary>
        /// Sign up agent user
        /// </summary>
        /// <param name="identity">UserIdentity</param>
        /// <returns>true on success</returns>
        Task<bool> SignUp(UserIdentity identity);

        /// <summary>
        /// GetChannels - Get list of device nodes
        /// </summary>
        /// <returns>List of {EltraDevice}</returns>
        Task<List<Channel>> GetChannels();

        /// <summary>
        /// Bind agent channel with device node channel using device credentials
        /// </summary>
        /// <param name="identity">device credentials</param>
        /// <returns>{bool}</returns>
        Task<bool> BindChannels(UserIdentity identity);

        #endregion
    }
}

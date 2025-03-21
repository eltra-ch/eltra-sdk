﻿using EltraCommon.Contracts.Users;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;
using System;
using System.Threading;
using System.Threading.Tasks;
using EltraConnector.Master.Events;
using EltraConnector.Master.Device;
using EltraCommon.Ipc;
using EltraConnector.Master.Device.Connection;
using EltraConnector.Master.Definitions;
using EltraConnector.Events;
using EltraCommon.Transport;
using EltraConnector.Transport.Ws;
using EltraConnector.Transport.Udp;

namespace EltraConnector.Master
{
    /// <summary>
    /// EltraMasterConnector
    /// </summary>
    public class EltraMasterConnector : IDisposable
    {
        #region Private fields

        private readonly IHttpClient _httpClient;
        private readonly IUdpClient _udpClient;
        private readonly IWebSocketClient _webSocketClient;

        private MasterStatus _status = MasterStatus.Undefined;
        private CancellationTokenSource _cancellationTokenSource;
        private bool disposedValue;
        private Authentication _authentication;

        #endregion

        #region Constructors

        /// <summary>
        /// EltraMasterConnector
        /// </summary>
        public EltraMasterConnector()
        {
            _httpClient = new EltraHttpClient();
            _udpClient = new EltraUdpClient();
            _webSocketClient = new EltraWebSocketClient();

            _cancellationTokenSource = new CancellationTokenSource();
            ConnectionSettings = new ConnectionSettings();
        }

        /// <summary>
        /// EltraMasterConnector
        /// </summary>
        public EltraMasterConnector(IHttpClient httpClient, IUdpClient udpClient, IWebSocketClient webSocketClient)
        {
            _httpClient = httpClient;
            _udpClient = udpClient;
            _webSocketClient = webSocketClient;

            _cancellationTokenSource = new CancellationTokenSource();
            ConnectionSettings = new ConnectionSettings();
        }

        #endregion

        #region Properties

        /// <summary>
        /// User defined channelId
        /// </summary>
        public string ChannelId { get; private set; }

        /// <summary>
        /// Authorization data
        /// </summary>
        public UserIdentity Identity { get; private set; }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// ConnectionSettings
        /// </summary>
        public ConnectionSettings ConnectionSettings { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public MasterStatus Status 
        { 
            get => _status;
            set
            {
                _status = value;
                OnStatusChanged();
            }
        }

        /// <summary>
        /// Authentication
        /// </summary>
        protected Authentication Authentication => _authentication ?? (_authentication = CreateAuthentication());

        #endregion

        #region Events

        /// <summary>
        /// Agent status changed
        /// </summary>
        public event EventHandler<MasterStatusEventArgs> StatusChanged;
        /// <summary>
        /// Remote (device) channel status changed
        /// </summary>
        public event EventHandler<AgentChannelStatusChangedEventArgs> RemoteChannelStatusChanged;
        /// <summary>
        /// Channel status changed
        /// </summary>
        public event EventHandler<AgentChannelStatusChangedEventArgs> ChannelStatusChanged;

        #endregion

        #region Events handling

        /// <summary>
        /// OnStatusChanged
        /// </summary>
        protected virtual void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, new MasterStatusEventArgs { Status = Status } );
        }

        /// <summary>
        /// OnStatusChanged
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStatusChanged(MasterStatusEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        private void OnAgentRemoteChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            RemoteChannelStatusChanged?.Invoke(sender, e);
        }

        private void OnAgentChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            ChannelStatusChanged?.Invoke(sender, e);
        }

        #endregion

        #region Methods

        private Authentication CreateAuthentication()
        {
            return new Authentication(_httpClient, Host);
        }

        /// <summary>
        /// Sign in user
        /// </summary>
        /// <param name="identity">User identity</param>
        /// <param name="createAccount"></param>
        /// <returns>true on success</returns>
        public async Task<bool> SignIn(UserIdentity identity, bool createAccount = false)
        {
            bool result = false;
            const string method = "SignIn";

            if (createAccount && !await Authentication.SignUp(identity))
            {
                MsgLogger.WriteWarning($"{GetType().Name} - {method}", $"Sign up failed, user '{identity.Name}' exists?");
            }

            if (await Authentication.SignIn(identity))
            {
                Identity = identity;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Sign out agent user
        /// </summary>
        /// <returns>true on success</returns>
        public async Task<bool> SignOut()
        {
            var result = await Authentication.SignOut();

            return result;
        }

        /// <summary>
        /// Create unique device user alias.
        /// </summary>
        /// <param name="role">User role</param>
        /// <returns></returns>
        public async Task<UserIdentity> CreateAlias(string role)
        {
            var result = await Authentication.CreateAlias(role);

            return result;
        }

        /// <summary>
        /// Create user-defined alias
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateAlias(UserIdentity identity)
        {
            bool result = await Authentication.CreateAlias(identity);
            
            return result;
        }

        /// <summary>
        /// Start async connector
        /// </summary>
        /// <param name="deviceManager"></param>
        /// <returns></returns>
        public async Task Start(MasterDeviceManager deviceManager)
        {
            if (Status != MasterStatus.Stopped && Status != MasterStatus.Undefined)
            {
                MsgLogger.WriteError($"{GetType().Name} - Start", $"Master already running, status = {Status}");
            }
            else
            {
                _cancellationTokenSource = new CancellationTokenSource();

                int reconnectDelay = (int)TimeSpan.FromSeconds(ConnectionSettings.Timeout).TotalMilliseconds;

                do
                {
                    await StartDeviceManager(deviceManager, reconnectDelay);

                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(reconnectDelay);
                    }
                }
                while (!_cancellationTokenSource.IsCancellationRequested);
                
                Status = MasterStatus.Stopped;
            }
        }

        private async Task StartDeviceManager(MasterDeviceManager deviceManager, int reconnectDelay)
        {
            try
            {
                SyncCloudAgent agent = null;

                MsgLogger.Print($"{GetType().Name} - Start", $"Sign in '{Identity.Login}' ...");

                if (await SignIn(Identity, false))
                {
                    MsgLogger.Print($"{GetType().Name} - Start", $"'{Identity.Login}' signed in successfully");

                    ChannelId = await Authentication.GetChannelId();

                    if (!string.IsNullOrEmpty(ChannelId))
                    {
                        agent = new SyncCloudAgent(_httpClient, _udpClient, _webSocketClient, Host, Identity, ChannelId, ConnectionSettings.UpdateInterval, ConnectionSettings.Timeout);
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Start", $"Master - channel cannot be read, quit");
                    }

                    if (agent != null)
                    {
                        await StartAgent(deviceManager, agent, reconnectDelay);
                    }
                    else
                    {
                        await Authentication.SignOut();

                        MsgLogger.Print($"{GetType().Name} - Start", "Disconnected");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Start", "Authentication failed, wrong password!");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Start", e);
            }
        }

        private async Task StartAgent(MasterDeviceManager deviceManager, SyncCloudAgent agent, int reconnectDelay)
        {
            const int minWaitTime = 100;

            RegisterEvents(agent);

            do
            {
                Status = MasterStatus.Starting;

                MsgLogger.Print($"{GetType().Name} - Start", $"Sign in '{Identity.Login}' ...");

                if (await SignIn(agent))
                {
                    MsgLogger.Print($"{GetType().Name} - Start", $"'{Identity.Login}' signed in successfully");

                    deviceManager.CloudAgent = agent;

                    MsgLogger.Print($"{GetType().Name} - Start", "scan devices...");

                    if (await deviceManager.Run())
                    {
                        Status = MasterStatus.Started;

                        while (!_cancellationTokenSource.IsCancellationRequested)
                        {
                            await Task.Delay(minWaitTime).ConfigureAwait(false);
                        }
                    }

                    Status = MasterStatus.Stopping;

                    MsgLogger.Print($"{GetType().Name} - Start", "Disconnect ...");

                    agent.Stop();

                    MsgLogger.Print($"{GetType().Name} - Start", $"Stop device manager");

                    await deviceManager.Stop();

                    MsgLogger.Print($"{GetType().Name} - Start", $"Sign out, login '{Identity.Login}' ...");

                    await agent.SignOut();

                    MsgLogger.Print($"{GetType().Name} - Start", "Disconnected");
                }
                else if (!agent.Good)
                {
                    MsgLogger.WriteError($"{GetType().Name} - Start", $"Connection failed, repeat in {ConnectionSettings.Timeout} sec. !");

                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(reconnectDelay);
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Start", "Authentication failed, wrong password!");

                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(reconnectDelay);
                    }
                }
            }
            while (!_cancellationTokenSource.IsCancellationRequested);

            UnregisterEvents(agent);
        }

        /// <summary>
        /// Stop connector
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            bool result = false;

            if (Status == MasterStatus.Starting || Status == MasterStatus.Started)
            {
                _cancellationTokenSource.Cancel();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Start connector as service
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="deviceManager"></param>
        /// <returns></returns>
        public bool StartService(string appName, MasterDeviceManager deviceManager)
        {
            bool result = false;

            MsgLogger.WriteFlow($"{GetType().Name} - StartService", $"Start master '{appName}' in service mode");

            var npServer = new NpServer() { Name = appName };

            if (npServer.Start())
            {
                npServer.StepRequested += (sender, e) =>
                {
                    MsgLogger.WriteFlow($"{GetType().Name} - StartService", "stop request received ...");

                    Stop();
                };

                if(!string.IsNullOrEmpty(Identity.Login) && !string.IsNullOrEmpty(Identity.Password))
                {
                    result = true;

                    MsgLogger.WriteFlow($"{GetType().Name} - StartService", $"host='{Host}', user={Identity.Login}, pwd='{Identity.Password}'");

                    var task = Task.Run(async ()=>
                    {
                        await Start(deviceManager);
                    });

                    task.Wait();
                }
                else
                {
                    MsgLogger.WriteFlow($"{GetType().Name} - StartService", "you have to enter login and password to proceed!");
                }

                npServer.Stop();
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - StartService", "start failed!");
            }

            return result;
        }

        /// <summary>
        /// Stop running service
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public bool StopService(string appName)
        {
            bool result = false;
            const int requestTimeout = 3000;

            var client = new NpClient() { Name = appName, Timeout = requestTimeout };

            if (client.Stop())
            {
                MsgLogger.WriteFlow($"{GetType().Name} - StopService", "stop request sent successfully!");
                result = true;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - StopService", "stop request sending failed!");
            }

            return result;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                }

                disposedValue = true;
            }
        }

        private async Task<bool> SignIn(SyncCloudAgent agent)
        {
            bool result = false;

            if (!await agent.SignIn(Identity) && agent.Good)
            {
                if (await agent.SignUp(Identity) && agent.Good && await agent.SignIn(Identity) && agent.Good)
                {
                    result = true;
                }
            }
            else
            {
                result = agent.Good;
            }

            return result;
        }

        private void RegisterEvents(SyncCloudAgent agent)
        {
            agent.ChannelStatusChanged += OnAgentChannelStatusChanged;
            agent.RemoteChannelStatusChanged += OnAgentRemoteChannelStatusChanged;
        }

        private void UnregisterEvents(SyncCloudAgent agent)
        {
            agent.ChannelStatusChanged -= OnAgentChannelStatusChanged;
            agent.RemoteChannelStatusChanged -= OnAgentRemoteChannelStatusChanged;
        }

        #endregion
    }
}

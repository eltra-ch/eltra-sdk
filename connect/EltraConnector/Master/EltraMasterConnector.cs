using EltraCommon.Contracts.Users;
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

namespace EltraConnector.Master
{
    /// <summary>
    /// EltraMasterConnector
    /// </summary>
    public class EltraMasterConnector : IDisposable
    {
        #region Constructors

        private MasterStatus _status = MasterStatus.Undefined;
        private CancellationTokenSource _cancellationTokenSource;
        private bool disposedValue;

        #endregion

        #region Constructors

        /// <summary>
        /// EltraMasterConnector
        /// </summary>
        public EltraMasterConnector()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            ConnectionSettings = new ConnectionSettings();
        }

        #endregion

        #region Properties

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

        #endregion

        #region Methods

        /// <summary>
        /// Sign in user
        /// </summary>
        /// <param name="identity">User identity</param>
        /// <param name="createAccount"></param>
        /// <returns>true on success</returns>
        public async Task<bool> SignIn(UserIdentity identity, bool createAccount = false)
        {
            bool result = false;
            var authentication = new Authentication(Host);

            if (authentication != null)
            {
                if (await authentication.SignIn(identity))
                {
                    Identity = identity;
                    result = true;
                }
                else if (createAccount)
                {
                    if (await authentication.SignUp(identity))
                    {
                        if (await authentication.SignIn(identity))
                        {
                            Identity = identity;
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sign out agent user
        /// </summary>
        /// <returns>true on success</returns>
        public async Task<bool> SignOut()
        {
            bool result = false;
            var authentication = new Authentication(Host);

            if (authentication != null)
            {
                result = await authentication.SignOut();
            }

            return result;
        }

        /// <summary>
        /// Create unique device user alias.
        /// </summary>
        /// <param name="level">User level</param>
        /// <returns></returns>
        public async Task<UserIdentity> CreateAlias(string level)
        {
            UserIdentity result = null;
            var authentication = new Authentication(Host);

            if (authentication != null)
            {
                result = await authentication.CreateAlias(level);
            }

            return result;
        }

        /// <summary>
        /// Create user-defined alias
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateAlias(UserIdentity identity)
        {
            bool result = false;
            var authentication = new Authentication(Host);

            if (authentication != null)
            {
                result = await authentication.CreateAlias(identity);
            }

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

                try
                {
                    var agent = new SyncCloudAgent(Host, Identity, ConnectionSettings.UpdateInterval, ConnectionSettings.Timeout);

                    RegisterEvents(agent);

                    bool repeat = false;

                    do
                    {
                        Status = MasterStatus.Starting;

                        MsgLogger.Print($"Sign in '{Identity.Login}' ...");

                        if (await SignIn(agent))
                        {
                            MsgLogger.Print($"'{Identity.Login}' signed in successfully");

                            deviceManager.CloudAgent = agent;

                            MsgLogger.Print("scan devices...");

                            await deviceManager.Run();

                            Status = MasterStatus.Started;

                            while (!_cancellationTokenSource.IsCancellationRequested)
                            {
                                await Task.Delay(100);
                            }

                            Status = MasterStatus.Stopping;

                            MsgLogger.Print("Disconnect ...");

                            agent.Stop();

                            MsgLogger.Print($"Stop device manager");

                            await deviceManager.Stop();

                            MsgLogger.Print($"Sign out, login '{Identity.Login}' ...");

                            await agent.SignOut();

                            MsgLogger.Print("Disconnected");
                        }
                        else if (!agent.Good)
                        {
                            repeat = true;

                            MsgLogger.WriteError($"{GetType().Name} - Start", $"Connection failed, repeat in {ConnectionSettings.Timeout} sec. !");

                            await Task.Delay(reconnectDelay);
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Start", "Authentication failed, wrong password!");
                        }
                    }
                    while (repeat && !_cancellationTokenSource.IsCancellationRequested);
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Start", e);
                }

                Status = MasterStatus.Stopped;
            }
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

            MsgLogger.WriteFlow($"Start master '{appName}' in service mode");

            var npServer = new NpServer() { Name = appName };

            if (npServer.Start())
            {
                npServer.StepRequested += (sender, e) =>
                {
                    MsgLogger.WriteFlow("stop request received ...");

                    Stop();
                };

                if(!string.IsNullOrEmpty(Identity.Login) && !string.IsNullOrEmpty(Identity.Password))
                {
                    result = true;

                    MsgLogger.WriteFlow($"host='{Host}', user={Identity.Login}, pwd='{Identity.Password}'");

                    var task = Task.Run(async ()=>
                    {
                        await Start(deviceManager);
                    });

                    task.Wait();
                }
                else
                {
                    MsgLogger.WriteFlow("you have to enter login and password to proceed!");
                }

                npServer.Stop();
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - Start", "start failed!");
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
                MsgLogger.WriteFlow("stop request sent successfully!");
                result = true;
            }
            else
            {
                MsgLogger.WriteError("RunOptionsAndReturnExitCode", "stop request sending failed!");
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
                }

                disposedValue = true;
            }
        }

        private async Task<bool> SignIn(SyncCloudAgent agent)
        {
            bool result = false;

            if (!await agent.SignIn(Identity) && agent.Good)
            {
                if (await agent.SignUp(Identity) && agent.Good)
                {
                    if (await agent.SignIn(Identity) && agent.Good)
                    {
                        result = true;
                    }
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
            agent.ChannelStatusChanged += (sender, args) =>
            {
                ChannelStatusChanged?.Invoke(sender, args);
            };

            agent.RemoteChannelStatusChanged += (sender, args) =>
            {
                RemoteChannelStatusChanged?.Invoke(sender, args);
            };
        }

        #endregion
    }
}

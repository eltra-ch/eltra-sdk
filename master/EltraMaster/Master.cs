using EltraCloudContracts.Contracts.Users;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;
using System;
using System.Threading;
using System.Threading.Tasks;
using EltraMaster.Status;
using EltraMaster.Events;
using EltraMaster.Device;
using EltraCommon.Ipc;

namespace EltraMaster
{
    public class EltraMasterConnector
    {
        #region Constructors

        private MasterStatus _status = MasterStatus.Undefined;
        private CancellationTokenSource _cancellationTokenSource;
        
        #endregion

        #region Constructors

        public EltraMasterConnector()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Properties

        public UserAuthData AuthData { get; set; }

        public string Host { get; set; }

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

        public event EventHandler<MasterStatusEventArgs> StatusChanged;

        #endregion

        #region Events handling

        protected virtual void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, new MasterStatusEventArgs { Status = Status } );
        }

        protected virtual void OnStatusChanged(MasterStatusEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public async Task Start(MasterDeviceManager deviceManager, uint updateInterval, uint timeout)
        {
            if (Status != MasterStatus.Stopped && Status != MasterStatus.Undefined)
            {
                MsgLogger.WriteError($"{GetType().Name} - Start", $"Master already running, status = {Status}");
            }
            else
            {
                _cancellationTokenSource = new CancellationTokenSource();

                int reconnectDelay = (int)TimeSpan.FromSeconds(timeout).TotalMilliseconds;

                try
                {
                    var agent = new SyncCloudAgent(Host, AuthData, updateInterval, timeout);

                    bool repeat = false;

                    do
                    {
                        Status = MasterStatus.Starting;

                        MsgLogger.Print($"Sign in '{AuthData.Login}' ...");

                        if (await SignIn(agent))
                        {
                            MsgLogger.Print($"'{AuthData.Login}' signed in successfully");

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

                            MsgLogger.Print($"Sign out, login '{AuthData.Login}' ...");

                            await agent.SignOut();

                            MsgLogger.Print("Disconnected");
                        }
                        else if (!agent.Good)
                        {
                            repeat = true;

                            MsgLogger.WriteError($"{GetType().Name} - Start", $"Connection failed, repeat in {timeout} sec. !");

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

        public bool StartService(string appName, MasterDeviceManager deviceManager, uint updateInterval, uint timeout)
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

                var authData = AuthData;

                if (MasterConsole.MasterConsole.CheckAuthData(ref authData))
                {
                    AuthData = authData;

                    result = true;

                    MsgLogger.WriteFlow($"host='{Host}', user={AuthData.Login}, pwd='{AuthData.Password}'");

                    var task = Task.Run(async ()=>
                    {
                        await Start(deviceManager, updateInterval, timeout);
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

        private async Task<bool> SignIn(SyncCloudAgent agent)
        {
            bool result = false;

            if (!await agent.SignIn(AuthData) && agent.Good)
            {
                if (await agent.SignUp(AuthData) && agent.Good)
                {
                    if (await agent.SignIn(AuthData) && agent.Good)
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

        #endregion
    }
}

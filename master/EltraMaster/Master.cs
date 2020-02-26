using EltraCloudContracts.Contracts.Users;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;
using System;
using System.Threading;
using System.Threading.Tasks;
using EltraMaster.Status;
using EltraMaster.Events;
using EltraMaster.Device;

namespace EltraMaster
{
    public class Master
    {
        #region Constructors

        private MasterStatus _status = MasterStatus.Undefined;
        private CancellationTokenSource _cancellationTokenSource;
        
        #endregion

        #region Constructors

        public Master()
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

        public async Task Start(string host, UserAuthData authData, MasterDeviceManager deviceManager, uint updateInterval, uint timeout)
        {
            int reconnectDelay = (int)TimeSpan.FromSeconds(timeout).TotalMilliseconds;

            Host = host;
            AuthData = authData;

            try
            {
                var agent = new SyncCloudAgent(Host, AuthData, updateInterval, timeout);

                bool repeat = false;

                do
                {
                    if (await agent.Login(AuthData) && agent.Good)
                    {
                        if (await agent.IsAuthValid() && agent.Good)
                        {
                            MsgLogger.Print($"Sign in '{AuthData.Login}' ...");

                            if (await agent.SignIn() && agent.Good)
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

                                MsgLogger.Print($"Sign out, login '{AuthData.Login}' ...");

                                await agent.SignOut();
                            }
                            else if (!agent.Good)
                            {
                                repeat = true;

                                MsgLogger.WriteError($"{GetType().Name} - Start", "Connection failed, repeat!");

                                await Task.Delay(reconnectDelay);
                            }
                            else
                            {
                                MsgLogger.WriteError($"{GetType().Name} - Start", "Sign in failed!");
                            }
                        }
                        else if (!agent.Good)
                        {
                            repeat = true;

                            MsgLogger.WriteError($"{GetType().Name} - Start", "Connection failed, repeat!");

                            await Task.Delay(reconnectDelay);
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Start", "Authentication failed, wrong password!");
                        }
                    }
                    else if(!agent.Good)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Start", "Connection failed, repeat!");
                        
                        await Task.Delay(reconnectDelay);

                        repeat = true;
                    }
                }
                while (repeat && !_cancellationTokenSource.IsCancellationRequested);
                
                MsgLogger.Print("Disconnect ...");

                agent.Stop();

                MsgLogger.Print("Disconnected");
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Start", e);
            }

            Status = MasterStatus.Stopped;
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        #endregion
    }
}

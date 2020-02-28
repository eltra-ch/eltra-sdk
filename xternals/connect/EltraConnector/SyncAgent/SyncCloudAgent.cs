using EltraConnector.Controllers;
using EltraConnector.Events;
using EltraConnector.Ws;
using EltraConnector.Sessions;
using EltraCommon.Threads;
using System;
using EltraCloudContracts.Contracts.Sessions;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Users;
using System.Threading.Tasks;
using EltraCloudContracts.Contracts.Devices;
using EltraConnector.Controllers.Base.Events;

namespace EltraConnector.SyncAgent
{
    public class SyncCloudAgent : EltraThread
    {
        #region Private fields

        private readonly DeviceSessionControllerAdapter _sessionControllerAdapter;
        
        private readonly SessionUpdater _sessionUpdater;
        private readonly CommandExecutor _commandExecutor;
        private readonly WsConnectionManager _wsConnectionManager;
        private readonly Authentication _authentication;
        private bool _good;

        #endregion

        #region Constructors

        public SyncCloudAgent(string url, UserAuthData authData, uint updateInterval, uint timeout)
        {
            Url = url;
            AuthData = authData;

            _good = true;
            _authentication = new Authentication(url);
            _wsConnectionManager = new WsConnectionManager() { HostUrl = url };

            _sessionControllerAdapter = new DeviceSessionControllerAdapter(url, authData, updateInterval, timeout) { WsConnectionManager = _wsConnectionManager };

            _sessionUpdater = new SessionUpdater(_sessionControllerAdapter, updateInterval, timeout);
            _commandExecutor = new CommandExecutor(_sessionControllerAdapter);

            RegisterEvents();
        }

        #endregion

        #region Events

        public event EventHandler<SessionStatusChangedEventArgs> RemoteSessionStatusChanged;

        public event EventHandler<GoodChangedEventArgs> GoodChanged;

        #endregion

        #region Events handling

        private void OnAdapterGoodChanged(object sender, GoodChangedEventArgs e)
        {
            Good = _sessionControllerAdapter.Good && _authentication.Good;
        }

        private void OnGoodChanged()
        {
            GoodChanged?.Invoke(this, new GoodChangedEventArgs() { Good = Good });
        }

        private void OnSessionRegistered(object sender, SessionRegistrationEventArgs e)
        {
            var session = e.Session;

            if (e.Success)
            {
                session.Status = SessionStatus.Online;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - OnSessionRegistered", $"Session ({session.Uuid}) registration failed!");
            }
        }

        protected virtual void OnRemoteSessionStatusChanged(object sender, SessionStatusChangedEventArgs e)
        {
            RemoteSessionStatusChanged?.Invoke(sender, e);
        }

        #endregion

        #region Properties

        public string Url { get; }

        public UserAuthData AuthData { get; }

        public bool Good
        {
            get => _good;
            set
            {
                if (_good != value)
                {
                    _good = value;

                    OnGoodChanged();
                }
            }
        }

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            _sessionControllerAdapter.SessionRegistered += OnSessionRegistered;
            _commandExecutor.RemoteSessionStatusChanged += OnRemoteSessionStatusChanged;

            _sessionControllerAdapter.GoodChanged += OnAdapterGoodChanged;
            _authentication.GoodChanged += OnAdapterGoodChanged;
        }

        private async Task<bool> RegisterSession()
        {
            bool result = false;

            if (!await _sessionControllerAdapter.IsSessionRegistered())
            {
                if (await _sessionControllerAdapter.RegisterSession())
                {
                    MsgLogger.WriteLine($"register session='{_sessionControllerAdapter.Session.Uuid}' success");
                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - RegisterSession", $"register session='{_sessionControllerAdapter.Session.Uuid}' failed!");
                }
            }
            else
            {
                result = true;
            }

            return result;
        }
        
        public async Task<bool> RegisterDevice(EltraDevice device)
        {
            bool result = false;

            try
            {
                if (await RegisterSession())
                {
                    MsgLogger.WriteLine(
                        $"register(+) device='{device.Family}', serial number=0x{device.Identification.SerialNumber:X}");

                    var registered = await _sessionControllerAdapter.IsDeviceRegistered(device);

                    if (!registered)
                    {
                        await _sessionControllerAdapter.RegisterDevice(device);
                    }

                    Start();
                    
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - RegisterDevice", e);
            }

            return result;
        }
        
        protected override async Task Execute()
        {
            int minWaitTime = 10;

            SetRunning();

            _sessionUpdater.Start();
            _commandExecutor.Start();

            while (ShouldRun())
            {
                await Task.Delay(minWaitTime);
            }

            _commandExecutor.Stop();
            _sessionUpdater.Stop();
            
            MsgLogger.WriteLine($"Sync agent working thread finished successfully!");

            SetStopped();            
        }
        
        public async Task UnregisterDevice(EltraDevice device)
        {
            try
            {
                var registered = await _sessionControllerAdapter.IsDeviceRegistered(device);
                
                if (registered)
                {
                    MsgLogger.WriteLine($"unregister(-) device='{device.Family}', serial number=0x{device.Identification.SerialNumber:X}");

                    await _sessionControllerAdapter.UnregisterDevice(device);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UnregisterDevice", e);
            }
        }

        public async Task<Session> GetSession(string uuid, UserAuthData authData)
        {
            Session result = null;

            try
            {
                result = await _sessionControllerAdapter.GetSession(uuid, authData);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetSession", e);
            }

            return result;
        }

        public async Task<bool> IsAuthValid(UserAuthData authData)
        {
            bool result = await _authentication.IsValid(authData);

            return result;
        }

        public async Task<bool> LoginExists(string login)
        {
            bool result = await _authentication.LoginExists(login);

            return result;
        }

        public async Task<string> SignIn(UserAuthData authData)
        {
            string result = await _authentication.SignIn(authData);

            return result;
        }

        public async Task<bool> SignOut(string token)
        {
            bool result = await _authentication.SignOut(token);

            return result;
        }

        public async Task<bool> Register(UserAuthData authData)
        {
            bool result = await _authentication.Register(authData);

            return result;
        }

        public override bool Stop()
        {
            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Stopping ...");

            _commandExecutor?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Command executor stopped 1/4");

            _sessionUpdater?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Session updater stopped 2/4");

            _sessionControllerAdapter?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Session controller stopped 3/4");

            _authentication?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Auth controller stopped 4/4");

            return base.Stop();
        }

        public async Task<bool> SignIn()
        {
            return await _authentication.SignIn();
        }

        public async Task<bool> SignOut()
        {
            return await _authentication.SignOut();
        }

        public async Task<bool> IsAuthValid()
        {
            return await _authentication.IsValid();
        }

        public async Task<bool> Login(UserAuthData authData)
        {
            return await _authentication.Login(authData);
        }

        #endregion
    }
}

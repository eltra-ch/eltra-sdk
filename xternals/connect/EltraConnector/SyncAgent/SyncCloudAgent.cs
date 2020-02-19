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

namespace EltraConnector.SyncAgent
{
    public class SyncCloudAgent : EltraThread
    {
        #region Private fields

        private readonly AuthControllerAdapter _authControllerAdapter;
        private readonly DeviceSessionControllerAdapter _sessionControllerAdapter;
        private readonly SessionUpdater _sessionUpdater;
        private readonly CommandExecutor _commandExecutor;
        private readonly WsConnectionManager _wsConnectionManager;
        private readonly string _url;
        private readonly UserAuthData _authData;
        private readonly Authentication _authentication;

        #endregion

        #region Constructors

        public SyncCloudAgent(string url, UserAuthData authData, uint updateInterval, uint timeout)
        {
            _url = url;
            _authData = authData;

            _authentication = new Authentication(this);
            _wsConnectionManager = new WsConnectionManager() { HostUrl = url };

            _authControllerAdapter = new AuthControllerAdapter(url);
            _sessionControllerAdapter = new DeviceSessionControllerAdapter(url, authData, updateInterval, timeout) { WsConnectionManager = _wsConnectionManager };

            _sessionUpdater = new SessionUpdater(_sessionControllerAdapter, updateInterval, timeout);
            _commandExecutor = new CommandExecutor(_sessionControllerAdapter);

            RegisterEvents();
        }

        #endregion

        #region Events

        public event EventHandler<SessionStatusChangedEventArgs> RemoteSessionStatusChanged;

        #endregion

        #region Events handling
               
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

        public string Url => _url;

        public UserAuthData AuthData => _authData;

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            _sessionControllerAdapter.SessionRegistered += OnSessionRegistered;
            _commandExecutor.RemoteSessionStatusChanged += OnRemoteSessionStatusChanged;
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
                        $"register(+) device='{device.Name}', serial number=0x{device.Identification.SerialNumber:X}");

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
                    MsgLogger.WriteLine($"unregister(-) device='{device.Name}', serial number=0x{device.Identification.SerialNumber:X}");

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
            bool result = false;

            try
            {
                result = await _authControllerAdapter.IsValid(authData.Login, authData.Password);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - IsAuthValid", e);
            }

            return result;
        }

        public async Task<bool> LoginExists(string login)
        {
            bool result = false;

            try
            {
                result = await _authControllerAdapter.LoginExists(login);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - LoginExists", e);
            }

            return result;
        }

        public async Task<string> SignIn(UserAuthData authData)
        {
            string result = string.Empty;

            try
            {
                result = await _authControllerAdapter.SignIn(authData);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignIn", e);
            }

            return result;
        }

        public async Task<bool> SignOut(string token)
        {
            bool result = false;

            try
            {
                result = await _authControllerAdapter.SignOut(token);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignOut", e);
            }

            return result;
        }

        public async Task<bool> Register(UserAuthData authData)
        {
            bool result = false;

            try
            {
                result = await _authControllerAdapter.Register(authData);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Register", e);
            }

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

            _authControllerAdapter?.Stop();

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

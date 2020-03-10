using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Users;
using EltraConnector.Controllers;
using System;
using EltraConnector.Controllers.Base.Events;

namespace EltraConnector.SyncAgent
{
    class Authentication
    {
        #region Private fields

        private readonly AuthControllerAdapter _authControllerAdapter;

        private UserAuthData _authData;
        private bool _good;

        #endregion

        #region Constructors

        public Authentication(string url)
        {
            _good = true;

            _authControllerAdapter = new AuthControllerAdapter(url);

            _authControllerAdapter.GoodChanged += OnAuthControllerAdapterGoodChanged;
        }

        #endregion

        #region Properties

        public bool Good
        {
            get => _good;
            set
            {
                _good = value;
                OnGoodChanged();
            }
        }

        #endregion

        #region Events

        public event EventHandler<GoodChangedEventArgs> GoodChanged;

        #endregion

        #region Events handling

        private void OnGoodChanged()
        {
            GoodChanged?.Invoke(this, new GoodChangedEventArgs() { Good = Good });
        }

        private void OnAuthControllerAdapterGoodChanged(object sender, Controllers.Base.Events.GoodChangedEventArgs e)
        {
            Good = e.Good;
        }

        #endregion

        #region Methods

        public async Task<bool> SignIn()
        {
            bool result = await SignIn(_authData);

            return result;
        }

        public async Task<bool> SignOut()
        {
            return await SignOut(_authData.Login);
        }

        public async Task<bool> IsValid()
        {
            return await IsValid(_authData);
        }

        public async Task<bool> SignIn(UserAuthData authData)
        {
            bool result = false;

            _authData = authData;

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

        private async Task<bool> SignUp(UserAuthData authData)
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

        public async Task<bool> IsValid(UserAuthData authData)
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

        public async Task<bool> Register(UserAuthData authData)
        {
            bool result = true;

            _authData = authData;

            if (!await LoginExists(authData.Login) && Good)
            {
                if (await SignUp(authData))
                {
                    MsgLogger.Print($"New login {authData.Login} registered successfully");
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Login", $"Auth data registration failed!");
                    result = false;
                }
            }

            return result;
        }

        public void Stop()
        {
            _authControllerAdapter?.Stop();
        }

        #endregion
    }
}

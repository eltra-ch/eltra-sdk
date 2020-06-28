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

        public async Task<bool> SignOut()
        {
            bool result = false;

            try
            {
                result = await _authControllerAdapter.SignOut();
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignOut", e);
            }

            return result;
        }

        public async Task<bool> SignUp(UserAuthData authData)
        {
            bool result = false;

            try
            {
                result = await _authControllerAdapter.SignUp(authData);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignUp", e);
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

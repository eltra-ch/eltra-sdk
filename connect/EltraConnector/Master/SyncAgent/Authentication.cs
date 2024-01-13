using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraCommon.Contracts.Users;
using EltraConnector.Controllers;
using System;
using EltraCommon.Transport.Events;
using EltraCommon.Transport;

namespace EltraConnector.SyncAgent
{
    /// <summary>
    /// Authentication
    /// </summary>
    public class Authentication
    {
        #region Private fields

        private readonly AuthControllerAdapter _authControllerAdapter;
        private bool _good;

        #endregion

        #region Constructors

        /// <summary>
        /// Authentication
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url"></param>
        public Authentication(IHttpClient httpClient, string url)
        {
            _good = true;

            _authControllerAdapter = new AuthControllerAdapter(httpClient, url);

            _authControllerAdapter.GoodChanged += OnAuthControllerAdapterGoodChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Good
        /// </summary>
        public bool Good
        {
            get => _good;
            set
            {
                _good = value;
                OnGoodChanged();
            }
        }

        /// <summary>
        /// Identity
        /// </summary>
        public UserIdentity Identity { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Good flag changed
        /// </summary>
        public event EventHandler<GoodChangedEventArgs> GoodChanged;

        #endregion

        #region Events handling

        private void OnGoodChanged()
        {
            GoodChanged?.Invoke(this, new GoodChangedEventArgs() { Good = Good });
        }

        private void OnAuthControllerAdapterGoodChanged(object sender, GoodChangedEventArgs e)
        {
            Good = e.Good;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sign-in
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public async Task<bool> SignIn(UserIdentity identity)
        {
            bool result = false;

            Identity = identity;

            try
            {
                result = await _authControllerAdapter.SignIn(identity);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignIn", e);
            }

            return result;
        }

        /// <summary>
        /// Sign-out
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Sign-off
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SignOff()
        {
            bool result = false;

            try
            {
                result = await _authControllerAdapter.SignOff();
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignOff", e);
            }

            return result;
        }

        /// <summary>
        /// Sign-up
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public async Task<bool> SignUp(UserIdentity identity)
        {
            bool result = false;

            try
            {
                result = await _authControllerAdapter.SignUp(identity);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignUp", e);
            }

            return result;
        }

        internal async Task<UserIdentity> CreateAlias(string role)
        {
            UserIdentity result = null;

            try
            {
                result = await _authControllerAdapter.CreateAlias(role);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CreateAlias", e);
            }

            return result;
        }

        internal async Task<bool> CreateAlias(UserIdentity identity)
        {
            bool result = false;

            try
            {
                result = await _authControllerAdapter.CreateAlias(identity);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CreateAlias", e);
            }

            return result;
        }

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            _authControllerAdapter?.Stop();
        }

        internal async Task<string> GetChannelId()
        {
            string result = string.Empty;

            try
            {
                result = await _authControllerAdapter.GetChannelId();
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetChannelId", e);
            }

            return result;
        }

        #endregion
    }
}

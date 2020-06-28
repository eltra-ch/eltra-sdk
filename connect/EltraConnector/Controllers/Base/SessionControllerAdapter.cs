using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.Contracts.Users;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraConnector.Events;
using EltraConnector.Extensions;
using EltraConnector.Ws;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace EltraConnector.Controllers.Base
{
    class SessionControllerAdapter : CloudControllerAdapter
    {
        #region Private fields

        private Session _session;
        private readonly User _user;
        private readonly string _uuid;
        private readonly uint _timeout;
        private readonly uint _updateInterval;

        #endregion

        #region Constructors

        public SessionControllerAdapter(string url, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url)
        {
            _timeout = timeout;
            _updateInterval = updateInterval;
            _uuid = Guid.NewGuid().ToString();
            _user = new User(authData) { Status = UserStatus.Unlocked };
        }

        public SessionControllerAdapter(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url)
        {
            _timeout = timeout;
            _updateInterval = updateInterval;
            _uuid = uuid;
            _user = new User(authData) { Status = UserStatus.Unlocked };
        }

        #endregion

        #region Events

        public event EventHandler<SessionRegistrationEventArgs> SessionRegistered;

        #endregion

        #region Events handling

        protected virtual void OnSessionRegistered(SessionRegistrationEventArgs e)
        {
            SessionRegistered?.Invoke(this, e);
        }

        #endregion

        #region Properties

        public Session Session => _session ?? (_session = new Session { Uuid = _uuid, User = _user, Timeout = _timeout, UpdateInterval = _updateInterval });

        public WsConnectionManager WsConnectionManager { get; set; }

        protected User User => _user;

        #endregion

        #region Methods

        public virtual async Task<bool> Update()
        {
            bool result = false;

            if (await IsSessionRegistered(Session.Uuid))
            {
                result = await SetSessionStatus(SessionStatus.Online);
            }
            else
            {
                if (await RegisterSession())
                {
                    result = await SetSessionStatus(SessionStatus.Online);
                }
            }

            return result;
        }

        public async Task<Session> GetSession(string uuid, UserAuthData authData)
        {
            Session result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uuid"] = uuid;
                query["login"] = authData.Login;
                query["password"] = authData.Password;

                var url = UrlHelper.BuildUrl(Url, "api/session/session", query);

                var json = await Transporter.Get(url);

                result = JsonConvert.DeserializeObject<Session>(json);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetSession", e);
            }

            return result;
        }

        public async Task<List<Session>> GetSessions(string uuid, UserAuthData authData)
        {
            var result = new List<Session>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uuid"] = uuid;
                query["login"] = authData.Login;
                query["password"] = authData.Password;

                var url = UrlHelper.BuildUrl(Url, "api/session/sessions", query);

                var json = await Transporter.Get(url);

                result = JsonConvert.DeserializeObject<List<Session>>(json);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetSessions", e);
            }

            return result;
        }

        public async Task<bool> RegisterSession()
        {
            bool result = false;

            try
            {
                var path = "api/session/add";
                var postResult = await Transporter.Post(Url, path, Session.ToJson());

                if (postResult.StatusCode == HttpStatusCode.OK)
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(postResult.Content);

                    result = requestResult.Result;
                }

                OnSessionRegistered(new SessionRegistrationEventArgs { Session = Session, Success = result, Exception = postResult.Exception });
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - RegisterSession", e);

                OnSessionRegistered(new SessionRegistrationEventArgs { Session = Session, Success = false, Exception = e });
            }

            return result;
        }

        public async Task<bool> UnregisterSession()
        {
            bool result = false;

            if (await IsSessionRegistered())
            {
                result = await SetSessionStatus(SessionStatus.Offline);
            }

            return result;
        }

        public async Task<bool> IsSessionRegistered()
        {
            bool result = await IsSessionRegistered(Session.Uuid);

            return result;
        }

        public async Task<bool> IsSessionRegistered(string sessionUuid)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uuid"] = Session.Uuid;
                query["SessionUuid"] = sessionUuid;

                var url = UrlHelper.BuildUrl(Url, "api/session/exists", query);

                var json = await Transporter.Get(url);

                var requestResult = JsonConvert.DeserializeObject<RequestResult>(json);

                if (requestResult != null)
                {
                    result = requestResult.Result;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - IsSessionRegistered", e);
            }

            return result;
        }


        protected async Task<bool> SetSessionStatus(SessionStatus status)
        {
            bool result = false;

            try
            {
                var statusUpdate = new SessionStatusUpdate { SessionUuid = Session.Uuid, Status = status };

                if (WsConnectionManager != null && WsConnectionManager.IsConnected(Session.Uuid))
                {
                    if (await WsConnectionManager.Send(Session.Uuid, statusUpdate))
                    {
                        var requestResult = await WsConnectionManager.Receive<RequestResult>(Session.Uuid);

                        if (requestResult != null)
                        {
                            result = requestResult.Result;
                        }
                    }
                }
                else
                {
                    var path = "api/session/status";

                    var json = JsonConvert.SerializeObject(statusUpdate);

                    var response = await Transporter.Put(Url, path, json);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var requestResult = JsonConvert.DeserializeObject<RequestResult>(response.Content);

                        result = requestResult.Result;
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        #endregion
    }
}

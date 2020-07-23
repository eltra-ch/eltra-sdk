using EltraCommon.Contracts.Results;
using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraConnector.Events;
using EltraConnector.Extensions;
using EltraConnector.Transport.Ws;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace EltraConnector.Controllers.Base
{
    class ChannelControllerAdapter : CloudControllerAdapter
    {
        #region Private fields

        private Channel _session;
        private readonly User _user;
        private readonly string _uuid;
        private readonly uint _timeout;
        private readonly uint _updateInterval;

        #endregion

        #region Constructors

        public ChannelControllerAdapter(string url, UserData authData, uint updateInterval, uint timeout)
            : base(url)
        {
            _timeout = timeout;
            _updateInterval = updateInterval;
            _uuid = Guid.NewGuid().ToString();
            _user = new User(authData) { Status = UserStatus.Unlocked };
        }

        public ChannelControllerAdapter(string url, string uuid, UserData authData, uint updateInterval, uint timeout)
            : base(url)
        {
            _timeout = timeout;
            _updateInterval = updateInterval;
            _uuid = uuid;
            _user = new User(authData) { Status = UserStatus.Unlocked };
        }

        #endregion

        #region Events

        public event EventHandler<ChannelRegistrationEventArgs> ChannelRegistered;

        #endregion

        #region Events handling

        protected virtual void OnChannelRegistered(ChannelRegistrationEventArgs e)
        {
            ChannelRegistered?.Invoke(this, e);
        }

        #endregion

        #region Properties

        public Channel Channel => _session ?? (_session = new Channel { Id = _uuid, User = _user, Timeout = _timeout, UpdateInterval = _updateInterval });

        public WsConnectionManager WsConnectionManager { get; set; }

        public User User => _user;

        #endregion

        #region Methods

        public virtual async Task<bool> Update()
        {
            bool result = false;

            if (await IsChannelRegistered(Channel.Id))
            {
                result = await SetChannelStatus(ChannelStatus.Online);
            }
            else
            {
                if (await RegisterChannel())
                {
                    result = await SetChannelStatus(ChannelStatus.Online);
                }
            }

            return result;
        }

        public async Task<Channel> GetChannel(string uuid, UserData authData)
        {
            Channel result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = uuid;
                query["login"] = authData.Login;
                query["password"] = authData.Password;

                var url = UrlHelper.BuildUrl(Url, "api/channel/channel", query);

                var json = await Transporter.Get(url);

                result = JsonConvert.DeserializeObject<Channel>(json);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetChannel", e);
            }

            return result;
        }

        public async Task<List<Channel>> GetChannels(string uuid, UserData authData)
        {
            var result = new List<Channel>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = uuid;
                query["login"] = authData.Login;
                query["password"] = authData.Password;

                var url = UrlHelper.BuildUrl(Url, "api/channel/channels", query);

                var json = await Transporter.Get(url);

                result = JsonConvert.DeserializeObject<List<Channel>>(json);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetChannels", e);
            }

            return result;
        }

        public async Task<bool> RegisterChannel()
        {
            bool result = false;

            try
            {
                var path = "api/channel/register";
                var postResult = await Transporter.Post(Url, path, Channel.ToJson());

                if (postResult.StatusCode == HttpStatusCode.OK)
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(postResult.Content);

                    result = requestResult.Result;
                }

                OnChannelRegistered(new ChannelRegistrationEventArgs { Channel = Channel, Success = result, Exception = postResult.Exception });
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - RegisterChannel", e);

                OnChannelRegistered(new ChannelRegistrationEventArgs { Channel = Channel, Success = false, Exception = e });
            }

            return result;
        }

        public async Task<bool> UnregisterChannel()
        {
            bool result = false;

            if (await IsChannelRegistered())
            {
                result = await SetChannelStatus(ChannelStatus.Offline);
            }

            return result;
        }

        public async Task<bool> IsChannelRegistered()
        {
            bool result = await IsChannelRegistered(Channel.Id);

            return result;
        }

        public async Task<bool> IsChannelRegistered(string channelId)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = Channel.Id;
                query["channelId"] = channelId;

                var url = UrlHelper.BuildUrl(Url, "api/channel/exists", query);

                var json = await Transporter.Get(url);

                var requestResult = JsonConvert.DeserializeObject<RequestResult>(json);

                if (requestResult != null)
                {
                    result = requestResult.Result;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - IsChannelRegistered", e);
            }

            return result;
        }


        protected async Task<bool> SetChannelStatus(ChannelStatus status)
        {
            bool result = false;

            try
            {
                var statusUpdate = new ChannelStatusUpdate { ChannelId = Channel.Id, Status = status, UserData = _user.UserData };

                if (WsConnectionManager != null && WsConnectionManager.IsConnected(Channel.Id))
                {
                    if (await WsConnectionManager.Send(Channel.Id, statusUpdate))
                    {
                        var requestResult = await WsConnectionManager.Receive<RequestResult>(Channel.Id);

                        if (requestResult != null)
                        {
                            result = requestResult.Result;
                        }
                    }
                }
                else
                {
                    var path = "api/channel/status";

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

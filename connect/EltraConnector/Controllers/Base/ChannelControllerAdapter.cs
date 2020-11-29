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
using System.Threading;
using EltraConnector.Helpers;

namespace EltraConnector.Controllers.Base
{
    class ChannelControllerAdapter : CloudControllerAdapter
    {
        #region Private fields

        private Channel _channel;
        private readonly User _user;
        private readonly string _uuid;
        private readonly uint _timeout;
        private readonly uint _updateInterval;

        #endregion

        #region Constructors

        public ChannelControllerAdapter(string url, UserIdentity identity, uint updateInterval, uint timeout)
            : base(url)
        {
            _timeout = timeout;
            _updateInterval = updateInterval;
            _uuid = Guid.NewGuid().ToString();
            _user = new User(identity) { Status = UserStatus.Unlocked };
        }

        public ChannelControllerAdapter(string url, string uuid, UserIdentity identity, uint updateInterval, uint timeout)
            : base(url)
        {
            _timeout = timeout;
            _updateInterval = updateInterval;
            _uuid = uuid;
            _user = new User(identity) { Status = UserStatus.Unlocked };
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

        public string ChannelId => Channel.Id;

        public Channel Channel => _channel ?? (_channel = new Channel { Id = _uuid, UserName = _user.Identity.Name, Timeout = _timeout, UpdateInterval = _updateInterval, LocalHost = "127.0.0.1" });

        public WsConnectionManager WsConnectionManager { get; set; }

        public User User => _user;

        public int UdpPort { get; set; }

        #endregion

        #region Methods

        public virtual async Task<bool> Update()
        {
            bool result = false;

            if (await IsChannelRegistered())
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

        public async Task<Channel> GetChannel(string uuid)
        {
            Channel result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["channelId"] = uuid;

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

        public async Task<bool> BindChannels(string uuid, UserIdentity identity)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = uuid;
                query["login"] = identity.Login;
                query["password"] = identity.Password;

                var url = UrlHelper.BuildUrl(Url, "api/channel/bind", query);

                result = await Transporter.Get(url, CancellationToken.None);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - BindChannels", e);
            }

            return result;
        }

        public async Task<bool> UnbindChannel(string callerId, string channelId)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = callerId;
                query["channelId"] = channelId;

                var url = UrlHelper.BuildUrl(Url, "api/channel/unbind", query);

                result = await Transporter.Get(url, CancellationToken.None);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UnbindChannel", e);
            }

            return result;
        }

        public async Task<List<Channel>> GetChannels()
        {
            var result = new List<Channel>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                var url = UrlHelper.BuildUrl(Url, "api/channel/channels", query);

                var json = await Transporter.Get(url);

                if(!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<List<Channel>>(json);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetChannels", e);
            }

            return result;
        }

        private void UpdateChannelLocalHost(ChannelBase channel)
        {
            if (channel != null && UdpPort > 0)
            {
                channel.LocalHost = $"{IpHelper.GetLocalIpAddress()}:{UdpPort}";
            }
        }
        private void UpdateChannelStatusLocalHost(ChannelStatusUpdate channelStatus)
        {
            if (channelStatus != null && UdpPort > 0)
            {
                channelStatus.LocalHost = $"{IpHelper.GetLocalIpAddress()}:{UdpPort}";
            }
        }

        public async Task<bool> RegisterChannel()
        {
            bool result = false;

            try
            {
                var path = "api/channel/register";

                var channelBase = (ChannelBase)Channel;

                UpdateChannelLocalHost(channelBase);

                var postResult = await Transporter.Post(Url, path, channelBase.ToJson());

                if (postResult.StatusCode == HttpStatusCode.OK)
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(postResult.Content);

                    result = requestResult.Result;
                }

                if(!result)
                {
                    Channel.Status = ChannelStatus.Offline;
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
            var status = await GetChannelStatus(Channel.Id);
            
            return status == ChannelStatus.Online || status == ChannelStatus.Offline;
        }

        public async Task<ChannelStatus> GetChannelStatus(string channelId)
        {
            var result = ChannelStatus.Undefined;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["channelId"] = channelId;

                var url = UrlHelper.BuildUrl(Url, "api/channel/status", query);

                var json = await Transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<ChannelStatus>(json);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetChannelStatus", e);
            }

            return result;
        }


        protected async Task<bool> SetChannelStatus(ChannelStatus status)
        {
            bool result = false;

            try
            {
                var statusUpdate = new ChannelStatusUpdate { ChannelId = Channel.Id, Status = status };

                UpdateChannelStatusLocalHost(statusUpdate);

                if (WsConnectionManager != null && WsConnectionManager.IsConnected(Channel.Id))
                {
                    if (await WsConnectionManager.Send(Channel.Id, _user.Identity, statusUpdate))
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

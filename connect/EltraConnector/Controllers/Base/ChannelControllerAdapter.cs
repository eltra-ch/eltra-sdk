using EltraCommon.Contracts.Results;
using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraConnector.Events;
using EltraCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Threading;
using EltraCommon.Transport;
using EltraConnector.Transport.Udp;
using EltraConnector.Transport.Ws.Interfaces;

namespace EltraConnector.Controllers.Base
{
    class ChannelControllerAdapter : CloudControllerAdapter
    {
        #region Private fields

        private Channel _channel;
        private readonly User _user;
        private readonly UserIdentity _identity;
        private readonly string _uuid;
        private readonly uint _timeout;
        private readonly uint _updateInterval;
        private IConnectionManager _connectionManager;

        #endregion

        #region Constructors

        public ChannelControllerAdapter(string url, UserIdentity identity, uint updateInterval, uint timeout)
            : base(url)
        {
            _uuid = Guid.NewGuid().ToString();
            _timeout = timeout;
            _updateInterval = updateInterval;            
            _user = new User(identity) { Status = UserStatus.Unlocked };
            _identity = identity;
        }

        public ChannelControllerAdapter(string url, string uuid, UserIdentity identity, uint updateInterval, uint timeout)
            : base(url)
        {
            _timeout = timeout;
            _updateInterval = updateInterval;
            _uuid = uuid;
            _user = new User(identity) { Status = UserStatus.Unlocked };
            _identity = identity;
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
        private void OnConnectionManagerChanged()
        {
            var t = Task.Run(async ()=> {

                if (ConnectionManager != null)
                {
                    if (await ConnectionManager.Connect(Channel.Id, "SessionUpdate"))
                    {
                        await SendChannelIdentyficationRequest();
                    }
                }
            });

            t.Wait();
        }

        #endregion

        #region Properties

        public string ChannelId => Channel.Id;

        public Channel Channel => _channel ?? (_channel = new Channel { Id = _uuid, UserName = _user.Identity.Name, Timeout = _timeout, UpdateInterval = _updateInterval, LocalHost = EltraUdpConnector.LocalHost });

        public IConnectionManager ConnectionManager 
        { 
            get => _connectionManager;
            set
            {
                _connectionManager = value;
                OnConnectionManagerChanged();
            }
        }

        public User User => _user;

        #endregion

        #region Methods

        private async Task<bool> SendChannelIdentyficationRequest()
        {
            bool result = false;

            if (ConnectionManager.IsConnected(Channel.Id))
            {
                var request = new ChannelIdentification() { Id = Channel.Id };

                result = await ConnectionManager.Send(Channel.Id, _identity, request);
            }

            return result;
        }

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
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Update", $"register channel failed!");
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

                var json = await Transporter.Get(_identity, url);

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

                result = await Transporter.Get(_identity, url, CancellationToken.None);
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

                result = await Transporter.Get(_identity, url, CancellationToken.None);
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

                var json = await Transporter.Get(_identity, url);

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
            if (channel != null)
            {
                var commandExecConnectionName = $"{ChannelId}_CommandExec";

                var commandExecConnection = ConnectionManager.GetConnection<UdpServerConnection>(commandExecConnectionName);

                if (commandExecConnection is UdpServerConnection connection)
                {
                    channel.LocalHost = connection.LocalHost;
                }
                else
                {
                    channel.LocalHost = EltraUdpConnector.LocalHost;
                }
            }
        }
        private void UpdateChannelStatusLocalHost(ChannelStatusUpdate channelStatus)
        {
            if (channelStatus != null)
            {
                var commandExecConnectionName = $"{ChannelId}_CommandExec";

                var commandExecConnection = ConnectionManager.GetConnection<UdpServerConnection>(commandExecConnectionName);

                if (commandExecConnection is UdpServerConnection connection)
                {
                    channelStatus.LocalHost = connection.LocalHost;
                }
                else
                {
                    channelStatus.LocalHost = EltraUdpConnector.LocalHost;
                }
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

                var postResult = await Transporter.Post(_identity, Url, path, channelBase.ToJson());

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

            if (!string.IsNullOrEmpty(channelId))
            {
                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query["channelId"] = channelId;

                    var url = UrlHelper.BuildUrl(Url, "api/channel/status", query);

                    var json = await Transporter.Get(_identity, url);

                    if (!string.IsNullOrEmpty(json))
                    {
                        result = JsonConvert.DeserializeObject<ChannelStatus>(json);
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - GetChannelStatus", e);
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - GetChannelStatus", "channelId is not specified!");
            }

            return result;
        }

        private async Task<bool> SendStatusUpdate(ChannelStatusUpdate statusUpdate)
        {
            bool result = false;

            if (ConnectionManager != null && ConnectionManager.IsConnected(Channel.Id))
            {
                await ConnectionManager.Lock();

                if (await ConnectionManager.Send(Channel.Id, _user.Identity, statusUpdate))
                {
                    var requestResult = await ConnectionManager.Receive<RequestResult>(Channel.Id);

                    if (requestResult != null)
                    {
                        result = requestResult.Result;
                    }
                }

                ConnectionManager.Unlock();
            }

            return result;
        }

        protected async Task<bool> SetChannelStatus(ChannelStatus status)
        {
            bool result;

            MsgLogger.WriteLine($"{GetType().Name} - Update, set channel status {status}");

            try
            {
                var statusUpdate = new ChannelStatusUpdate { ChannelId = Channel.Id, Status = status };

                UpdateChannelStatusLocalHost(statusUpdate);

                result = await SendStatusUpdate(statusUpdate);
            }
            catch (Exception)
            {
                result = false;
            }

            if (!result)
            {
                MsgLogger.WriteError($"{GetType().Name} - Update", $"set channel status failed!");
            }

            return result;
        }

        #endregion
    }
}

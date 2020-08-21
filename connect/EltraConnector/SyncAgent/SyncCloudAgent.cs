using EltraConnector.Controllers;
using EltraConnector.Events;
using EltraConnector.Transport.Ws;
using EltraConnector.Sessions;
using EltraCommon.Threads;
using System;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Contracts.Users;
using System.Threading.Tasks;
using EltraConnector.Controllers.Base.Events;
using EltraCommon.Contracts.Devices;

#pragma warning disable 1591

namespace EltraConnector.SyncAgent
{
    public class SyncCloudAgent : EltraThread
    {
        #region Private fields

        private readonly DeviceChannelControllerAdapter _channelControllerAdapter;
        
        private readonly ChannelHeartbeat _channelHeartbeat;
        private readonly CommandExecutor _commandExecutor;
        private readonly WsConnectionManager _wsConnectionManager;
        private readonly Authentication _authentication;
        private bool _good;

        #endregion

        #region Constructors

        public SyncCloudAgent(string url, UserIdentity identity, uint updateInterval, uint timeout)
        {
            Url = url;
            Identity = identity;
            UpdateInterval = updateInterval;
            Timeout = timeout; 
            
            _good = true;
            _authentication = new Authentication(url);
            _wsConnectionManager = new WsConnectionManager() { HostUrl = url };

            _channelControllerAdapter = new DeviceChannelControllerAdapter(this) { WsConnectionManager = _wsConnectionManager };

            _channelHeartbeat = new ChannelHeartbeat(_channelControllerAdapter, updateInterval, timeout);
            _commandExecutor = new CommandExecutor(_channelControllerAdapter);

            RegisterEvents();
        }

        #endregion

        #region Events

        public event EventHandler<AgentChannelStatusChangedEventArgs> RemoteChannelStatusChanged;
        public event EventHandler<AgentChannelStatusChangedEventArgs> ChannelStatusChanged;

        public event EventHandler<GoodChangedEventArgs> GoodChanged;

        #endregion

        #region Events handling

        private void OnAdapterGoodChanged(object sender, GoodChangedEventArgs e)
        {
            Good = _channelControllerAdapter.Good && _authentication.Good;
        }

        private void OnGoodChanged()
        {
            GoodChanged?.Invoke(this, new GoodChangedEventArgs() { Good = Good });
        }

        private void OnChannelRegistered(object sender, ChannelRegistrationEventArgs e)
        {
            var channel = e.Channel;

            if (e.Success)
            {
                channel.Status = ChannelStatus.Online;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - OnChannelRegistered", $"Session ({channel.Id}) registration failed!");
            }
        }

        protected virtual void OnRemoteChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            RemoteChannelStatusChanged?.Invoke(sender, e);
        }
        protected virtual void OnChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            ChannelStatusChanged?.Invoke(sender, e);
        }

        #endregion

        #region Properties

        public string Url { get; }

        public UserIdentity Identity { get; }

        public uint UpdateInterval { get; set; }

        public uint Timeout { get; set; }

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

        public string ChannelId => _channelControllerAdapter.Channel.Id;

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            _channelHeartbeat.StatusChanged += OnChannelStatusChanged;

            _channelControllerAdapter.ChannelRegistered += OnChannelRegistered;
            _commandExecutor.RemoteChannelStatusChanged += OnRemoteChannelStatusChanged;

            _channelControllerAdapter.GoodChanged += OnAdapterGoodChanged;
            _authentication.GoodChanged += OnAdapterGoodChanged;
        }

        private async Task<bool> RegisterSession()
        {
            bool result = false;

            if (!await _channelControllerAdapter.IsChannelRegistered())
            {
                if (await _channelControllerAdapter.RegisterChannel())
                {
                    MsgLogger.WriteLine($"register session='{_channelControllerAdapter.Channel.Id}' success");
                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - RegisterSession", $"register session='{_channelControllerAdapter.Channel.Id}' failed!");
                }
            }
            else
            {
                result = true;
            }

            return result;
        }
        
        public async Task<bool> RegisterDevice(EltraDevice deviceNode)
        {
            bool result = false;

            try
            {
                var device = deviceNode;

                if (await RegisterSession())
                {
                    MsgLogger.WriteLine(
                        $"register(+) device='{device.Family}', node id = {device.NodeId}, serial number=0x{device.Identification.SerialNumber:X}");

                    var registered = await _channelControllerAdapter.IsDeviceRegistered(device);

                    if (!registered)
                    {
                        await _channelControllerAdapter.RegisterDevice(deviceNode);
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
        
        protected override Task Execute()
        {
            SetRunning();

            _channelHeartbeat.Start();
            _commandExecutor.Start();

            Wait();

            _commandExecutor.Stop();
            _channelHeartbeat.Stop();
            
            MsgLogger.WriteLine($"Sync agent working thread finished successfully!");

            SetStopped();

            return Task.CompletedTask;
        }
        
        public async Task<bool> UnregisterDevice(EltraDevice deviceNode)
        {
            bool result = false;

            try
            {
                var device = deviceNode;

                var registered = await _channelControllerAdapter.IsDeviceRegistered(device);
                
                if (registered)
                {
                    MsgLogger.WriteLine($"unregister(-) device='{device.Family}', node id = {device.NodeId}, serial number=0x{device.Identification.SerialNumber:X}");

                    result = await _channelControllerAdapter.UnregisterDevice(deviceNode);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UnregisterDevice", e);
            }

            return result;
        }

        public async Task<Channel> GetSession(string uuid, UserIdentity identity)
        {
            Channel result = null;

            try
            {
                result = await _channelControllerAdapter.GetChannel(uuid, identity);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetSession", e);
            }

            return result;
        }

        public async Task<bool> SignIn(UserIdentity identity)
        {
            bool result = await _authentication.SignIn(identity);

            return result;
        }

        public async Task<bool> SignOut()
        {
            bool result = await _authentication.SignOut();

            return result;
        }

        public async Task<bool> SignUp(UserIdentity identity)
        {
            bool result = await _authentication.SignUp(identity);

            return result;
        }

        public override bool Stop()
        {
            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Stopping ...");

            _commandExecutor?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Command executor stopped 1/4");

            _channelHeartbeat?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Session updater stopped 2/4");

            _channelControllerAdapter?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Session controller stopped 3/4");

            _authentication?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - Stop", "Auth controller stopped 4/4");

            return base.Stop();
        }

        #endregion
    }
}

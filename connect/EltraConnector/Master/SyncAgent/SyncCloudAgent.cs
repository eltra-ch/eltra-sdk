﻿using EltraConnector.Events;
using EltraCommon.Threads;
using System;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Contracts.Users;
using System.Threading.Tasks;
using EltraCommon.Contracts.Devices;
using EltraConnector.Channels;
using EltraCommon.Contracts.ToolSet;
using System.Net.Sockets;
using EltraCommon.Transport.Events;
using EltraConnector.Transport.Ws.Interfaces;
using EltraConnector.Master.Controllers;
using EltraConnector.Transport;
using EltraConnector.Master.Controllers.Commands;

#pragma warning disable 1591

namespace EltraConnector.SyncAgent
{
    public class SyncCloudAgent : EltraThread
    {
        #region Private fields

        private readonly MasterChannelControllerAdapter _channelControllerAdapter;
        
        private readonly ChannelHeartbeat _channelHeartbeat;
        private readonly MasterCommandExecutor _commandExecutor;
        private readonly IConnectionManager _connectionManager;
        private readonly Authentication _authentication;
        
        private bool _good;

        #endregion

        #region Constructors

        public SyncCloudAgent(string url, UserIdentity identity, string channelId, uint updateInterval, uint timeout)
        {
            Url = url;
            Identity = identity;
            UpdateInterval = updateInterval;
            Timeout = timeout;
            
            _good = true;
            _authentication = new Authentication(url);
            _connectionManager = new ConnectionManager() { HostUrl = url };

            _channelControllerAdapter = new MasterChannelControllerAdapter(this, channelId) { ConnectionManager = _connectionManager };

            _channelHeartbeat = new ChannelHeartbeat(_channelControllerAdapter, updateInterval, timeout);
            _commandExecutor = new MasterCommandExecutor(_channelControllerAdapter);

            RegisterEvents();
        }

        public SyncCloudAgent(string url, UserIdentity identity, uint updateInterval, uint timeout)
        {
            Url = url;
            Identity = identity;
            UpdateInterval = updateInterval;
            Timeout = timeout;

            _good = true;
            _authentication = new Authentication(url);
            _connectionManager = new ConnectionManager() { HostUrl = url };
            
            _channelControllerAdapter = new MasterChannelControllerAdapter(this) { ConnectionManager = _connectionManager };

            _channelHeartbeat = new ChannelHeartbeat(_channelControllerAdapter, updateInterval, timeout);
            _commandExecutor = new MasterCommandExecutor(_channelControllerAdapter);

            RegisterEvents();
        }

        #endregion

        #region Events

        public event EventHandler<AgentChannelStatusChangedEventArgs> RemoteChannelStatusChanged;
        public event EventHandler<AgentChannelStatusChangedEventArgs> ChannelStatusChanged;

        public event EventHandler<GoodChangedEventArgs> GoodChanged;

        #endregion

        #region Events handling

        private void OnUdpServerErrorRaised(object sender, SocketError e)
        {
            MsgLogger.WriteError($"{GetType().Name} - OnUdpServerErrorRaised", $"Udp server error occured  = {e}!");
        }

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

        private void OnSignInRequested(object sender, SignInRequestEventArgs args)
        {
            var t = Task.Run(async ()=> 
            {
                args.SignInResult = await SignIn(Identity); 
            });

            t.Wait();
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

        public IConnectionManager ConnectionManager => _connectionManager;

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            _channelHeartbeat.ChannelStatusChanged += OnChannelStatusChanged;
            _channelHeartbeat.SignInRequested += OnSignInRequested;

            _channelControllerAdapter.ChannelRegistered += OnChannelRegistered;
            
            _commandExecutor.RemoteChannelStatusChanged += OnRemoteChannelStatusChanged;
            _commandExecutor.SignInRequested += OnSignInRequested;

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

                    result = await _channelControllerAdapter.RegisterDevice(deviceNode);

                    if (result)
                    {
                        Start();
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - RegisterDevice", $"register device = '{device.Family}', node id = { device.NodeId}, serial number = 0x{ device.Identification.SerialNumber:X} failed!");
                    }
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

        public async Task<Channel> GetChannel(string uuid)
        {
            Channel result = null;

            try
            {
                result = await _channelControllerAdapter.GetChannel(uuid);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetChannel", e);
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

        public Task<bool> PayloadExists(DeviceToolPayload payload)
        {
            return _channelControllerAdapter.PayloadExists(payload);
        }

        public Task<bool> UploadPayload(DeviceToolPayload payload)
        {
            return _channelControllerAdapter.UploadPayload(payload);
        }

        #endregion
    }
}

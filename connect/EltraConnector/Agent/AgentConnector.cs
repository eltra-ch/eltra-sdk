﻿using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.Agent.Events;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Contracts.Channels;
using EltraConnector.UserAgent.Definitions;
using EltraConnector.SyncAgent;
using System.Diagnostics;
using EltraConnector.Interfaces;
using EltraCommon.Transport;
using EltraCommon.Logger;
using EltraConnector.Agent.UserAgent.Events;
using EltraConnector.Transport.Ws;
using EltraConnector.Transport.Udp;

namespace EltraConnector.Agent
{
    /// <summary>
    /// End-User Connector used by ELTRA IoT clients to communicate with Cloud Service
    /// </summary>
    public class AgentConnector : IDisposable, IAgentConnector
    {
        #region Private fields

        private readonly uint _updateInterval;
        private readonly IHttpClient _httpClient;
        private readonly IUdpClient _udpClient;
        private readonly IWebSocketClient _webSocketClient;
        private readonly List<(Channel, DeviceVcs)> _vcsList = new List<(Channel, DeviceVcs)>();

        private DeviceAgent _deviceAgent;
        private uint _timeout;
        private string _host;
        private UserIdentity _identity;
        private bool disposedValue;
        private AgentStatus _status;
        private AgentStatus _previousStatus;
        private Authentication _authentication;

        #endregion

        #region Constructors

        /// <summary>
        /// AgentConnector
        /// </summary>
        public AgentConnector()
        {
            _httpClient = new EltraHttpClient();
            _udpClient = new EltraUdpClient();
            _webSocketClient = new EltraWebSocketClient();

            _updateInterval = 60;
            _timeout = 180;
        }

        /// <summary>
        /// AgentConnector
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="udpClient"></param>
        /// <param name="webSocketClient"></param>
        public AgentConnector(IHttpClient httpClient, IUdpClient udpClient, IWebSocketClient webSocketClient)
        {
            _httpClient = httpClient;
            _udpClient = udpClient;
            _webSocketClient = webSocketClient;
            
            _updateInterval = 60;
            _timeout = 180;
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Cloud adapter
        /// </summary>
        public CloudControllerAdapter CloudAdapter => _deviceAgent?.Adapter;

        /// <summary>
        /// ELTRA Cloud IoT Service host name
        /// </summary>
        public string Host 
        { 
            get => _host;
            set
            {
                if (_host != value)
                {
                    _host = value;
                    OnHostChanged();
                }
            } 
        }

        /// <summary>
        /// Agent authorisation data 
        /// </summary>
        public UserIdentity Identity 
        { 
            get => _identity;
            private set 
            {
                if(_identity != value)
                {
                    _identity = value;
                    OnIdentityChanged();
                }
            } 
        }

        /// <summary>
        /// Agent channel
        /// </summary>
        public Channel Channel 
        { 
            get => _deviceAgent?.Channel;
        }

        /// <summary>
        /// Agent channel status
        /// </summary>
        public ChannelStatus ChannelStatus
        {
            get
            {
                var result = ChannelStatus.Undefined;

                if (Channel != null)
                {
                    result = Channel.Status;
                }

                return result;
            }
            private set
            {
                if (Channel != null)
                {
                    Channel.Status = value;
                }
            }
        }
        
        /// <summary>
        /// Agent status
        /// </summary>
        public AgentStatus Status
        {
            get => _status;
            private set 
            {
                if (_status != value)
                {
                    _previousStatus = _status;
                    _status = value;

                    OnStatusChanged();
                }
            }
        }

        /// <summary>
        /// Timeout in seconds (channel timeout, connection timeout)
        /// </summary>
        public uint Timeout
        {
            get => _timeout;
            set => _timeout = value;
        }

        private Authentication Authentication => _authentication ?? (_authentication = CreateAuthentication());

        #endregion

        #region Events

        /// <summary>
        /// Agent status changed
        /// </summary>
        public event EventHandler<AgentStatusEventArgs> StatusChanged;
        
        /// <summary>
        /// Device node detected
        /// </summary>
        public event EventHandler<DeviceDetectedEventArgs> DeviceDetected;

        #endregion

        #region Events handling

        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, new AgentStatusEventArgs { Status = Status });
        }

        private void OnHostChanged()
        {
            Disconnect();
        }

        private void OnIdentityChanged()
        {
            Disconnect();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connect
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Connect()
        {
            bool result = false;

            if (Host == null)
            {
                throw new ArgumentException("Host property not specified!");
            }

            if (Identity == null)
            {
                throw new ArgumentException("Identity property not specified!");
            }

            Disconnect();

            Status = AgentStatus.Starting;
            
            if(await CreateDeviceAgent())
            {
                RegisterEvents();

                result = await EnsureAgentReady();

                if (result)
                {
                    ChannelStatus = ChannelStatus.Online;
                }
            }
            else
            {
                Status = AgentStatus.Stopped;
            }
            

            return result;
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="deviceIdentity">UserIdentity</param>
        /// <returns></returns>
        public async Task<bool> Connect(UserIdentity deviceIdentity)
        {
            bool result = false;

            if (Host == null)
            {
                throw new ArgumentException("Host property not specified!");
            }

            if (Identity == null)
            {
                throw new ArgumentException("Identity property not specified!");
            }

            Disconnect();

            Status = AgentStatus.Starting;

            if (await CreateDeviceAgent())
            {
                RegisterEvents();

                if (await EnsureAgentReady())
                {
                    ChannelStatus = ChannelStatus.Online;

                    result = await BindChannels(deviceIdentity);
                }
            }
            else
            {
                Status = AgentStatus.Stopped;
            }

            return result;
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        public void Disconnect()
        {
            if (_deviceAgent != null)
            {
                Status = AgentStatus.Stopping;

                _deviceAgent.Dispose();
                _deviceAgent = null;

                Status = AgentStatus.Stopped;
            }
        }

        /// <summary>
        /// Sign in user
        /// </summary>
        /// <param name="identity">User identity</param>
        /// <param name="createAccount"></param>
        /// <returns>true on success</returns>
        public async Task<bool> SignIn(UserIdentity identity, bool createAccount = false)
        {
            bool result = false;
            
            if (await Authentication.SignIn(identity))
            {
                Identity = identity;
            
                result = true;

                Status = AgentStatus.SignedIn;
            }
            else if (createAccount && await Authentication.SignUp(identity))
            {
                Status = AgentStatus.SignedUp;

                if (await Authentication.SignIn(identity))
                {
                    Identity = identity;

                    result = true;

                    Status = AgentStatus.SignedIn;
                }
            }

            return result;
        }

        /// <summary>
        /// Sign out agent user
        /// </summary>
        /// <returns>true on success</returns>
        public async Task<bool> SignOut()
        {
            bool result = false;

            if (Identity != null)
            {
                result = await Authentication.SignOut();

                if (result)
                {
                    Status = AgentStatus.SignedOut;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Sign-off - removes the account.
        /// </summary>
        /// <returns>true of success</returns>
        public async Task<bool> SignOff()
        {
            bool result = await Authentication.SignOff();

            return result;
        }

        /// <summary>
        /// Sign up agent user
        /// </summary>
        /// <param name="identity">UserIdentity</param>
        /// <returns>true on success</returns>
        public async Task<bool> SignUp(UserIdentity identity)
        {
            bool result = await Authentication.SignUp(identity);

            return result;
        }

        /// <summary>
        /// GetChannels - Get list of device nodes
        /// </summary>
        /// <returns>List of {EltraDevice}</returns>
        public async Task<List<Channel>> GetChannels()
        {
            var result = new List<Channel>();

            if (Host == null)
            {
                throw new ArgumentException("Host property not specified!");
            }

            if (Identity==null)
            {
                throw new ArgumentException("Identity property not specified!");
            }

            _vcsList.Clear();

            if (_deviceAgent != null)
            {
                var channels = await _deviceAgent.GetChannels();

                foreach (var channel in channels)
                {
                    foreach (var device in channel.Devices)
                    {
                        var vcs = FindVcs(device);

                        if (vcs == null)
                        {
                            vcs = new DeviceVcs(_deviceAgent, device) { DeviceChannel = channel };

                            vcs.DeviceChanged += (sender, args) => { DeviceDetected?.Invoke(this, new DeviceDetectedEventArgs() { Device = device }); };

                            _vcsList.Add((channel, vcs));
                        }
                        else
                        {
                            vcs.DeviceChannel = channel;
                            vcs.Device = device;
                        }
                    }
                }

                result = channels;
            }

            return result;
        }

        /// <summary>
        /// Bind agent channel with device node channel using device credentials
        /// </summary>
        /// <param name="identity">device credentials</param>
        /// <returns>{bool}</returns>
        public async Task<bool> BindChannels(UserIdentity identity)
        {
            bool result = false;

            if (Host == null)
            {
                throw new ArgumentException("Host property not specified!");
            }

            if (Identity == null)
            {
                throw new ArgumentException("AuthData property not specified!");
            }

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.BindChannels(identity);

                if (result)
                {
                    Status = AgentStatus.Bound;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Unbind bounded channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<bool> UnbindChannel(Channel channel)
        {
            bool result = false;

            if (Host == null)
            {
                throw new ArgumentException("Host property not specified!");
            }

            if (Identity == null)
            {
                throw new ArgumentException("AuthData property not specified!");
            }

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.UnbindChannel(channel);

                if (result)
                {
                    Status = _previousStatus;
                }
            }

            return result;
        }

        /// <summary>
        /// SearchParameter - search device parameter by index and subindex
        /// </summary>
        /// <param name="device">Eltra device instance</param>
        /// <param name="index">parameter index</param>
        /// <param name="subIndex">parameter subindex</param>
        /// <returns>{ParameterBase}</returns>
        protected ParameterBase SearchParameter(EltraDevice device, ushort index, byte subIndex)
        {
            ParameterBase result = null;
            
            var vcs = SearchDeviceVcs(device);

            if(vcs!=null)
            {
                result = vcs.SearchParameter(index, subIndex);
            }

            return result;
        }

        /// <summary>
        /// SearchParameter - search device parameter by parameter uniqueId
        /// </summary>
        /// <param name="device">Eltra device instance</param>
        /// <param name="uniqueId">Parameter uniqueId</param>
        /// <returns>{ParameterBase}</returns>
        protected ParameterBase SearchParameter(EltraDevice device, string uniqueId)
        {
            ParameterBase result = null;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = vcs.SearchParameter(uniqueId);
            }

            return result;
        }

        /// <summary>
        /// Dispose agent instance
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && _deviceAgent != null)
                {
                    _deviceAgent.Dispose();
                    _deviceAgent = null;
                }

                disposedValue = true;
            }
        }

        private Channel FindChannelById(string channelId)
        {
            Channel result = null;

            foreach(var vcs in _vcsList)
            {
                if(vcs.Item1.Id == channelId)
                {
                    result = vcs.Item1;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// GetChannel
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public Channel GetChannel(EltraDevice device)
        {
            Channel result = null;
            var deviceVcs = FindDeviceVcs(device);

            if(deviceVcs!=null)
            {
                result = deviceVcs.DeviceChannel;
            }

            return result;
        }

        private DeviceVcs FindDeviceVcs(EltraDevice device)
        {
            DeviceVcs result = null;

            foreach (var vcs in _vcsList)
            {
                if (vcs.Item2.Device == device)
                {
                    result = vcs.Item2;
                    break;
                }
            }

            return result;
        }

        private DeviceVcs SearchDeviceVcs(EltraDevice device)
        {
            DeviceVcs result = FindDeviceVcs(device);

            if (result == null && device != null && _deviceAgent != null)
            {
                var channel = FindChannelById(device.ChannelId);
                var vcs = new DeviceVcs(_deviceAgent, device);

                vcs.DeviceChanged += (sender, args) => { DeviceDetected?.Invoke(this, new DeviceDetectedEventArgs() { Device = device }); };

                _vcsList.Add((channel, vcs));

                result = vcs;
            }

            return result;
        }

        private void RegisterEvents()
        {
            if (_deviceAgent != null)
            {
                _deviceAgent.StatusChanged += (sender, args) =>
                {
                    Status = args.Status;
                };

                _deviceAgent.AgentChannelStatusChanged += (sender, args) =>
                {
                    ChannelStatus = args.Status;
                };

                _deviceAgent.RemoteChannelStatusChanged += (sender, args) =>
                {
                    var channel = FindChannelById(args.Id);

                    if (channel != null)
                    {
                        channel.Status = args.Status;
                    }
                };
            }
        }

        private DeviceVcs FindVcs(EltraDevice device)
        {
            DeviceVcs result = null;

            if (device != null)
            {
                foreach (var vcs in _vcsList)
                {
                    var vcsDevice = vcs.Item2.Device;

                    if (vcsDevice != null &&
                        vcsDevice.ChannelId == device.ChannelId &&
                        vcsDevice.NodeId == device.NodeId)
                    {
                        result = vcs.Item2;
                        break;
                    }
                }
            }

            return result;
        }

        private async Task<bool> EnsureAgentReady()
        {
            const int minWaitTime = 10;
            bool result = false;

            if (Status == AgentStatus.Starting)
            {
                var waiter = new Stopwatch();

                waiter.Start();

                var timeout = TimeSpan.FromSeconds(_timeout);

                do
                {
                    await Task.Delay(minWaitTime);

                    if(_deviceAgent != null && _deviceAgent.Status == AgentStatus.Online)
                    {
                        Status = AgentStatus.Online;
                        break;
                    }

                    if (_deviceAgent != null && _deviceAgent.Status == AgentStatus.Started)
                    {
                        Status = AgentStatus.Started;
                        break;
                    }

                    if (_deviceAgent != null && _deviceAgent.Status == AgentStatus.Bound)
                    {
                        Status = AgentStatus.Bound;
                        break;
                    }

                } while (Status != AgentStatus.Online && Status != AgentStatus.Bound && waiter.Elapsed < timeout);

                result = waiter.Elapsed < timeout;
            }
            else if (Status == AgentStatus.Online || Status == AgentStatus.Started)
            {
                result = true;
            }

            return result;
        }

        internal virtual Authentication CreateAuthentication()
        {
            return new Authentication(_httpClient, Host);
        }


        private async Task<bool> CreateDeviceAgent()
        {
            bool result = false;
            const string method = "CreateDeviceAgent";

            try
            {
                if (_deviceAgent != null)
                {
                    _deviceAgent = new DeviceAgent(_httpClient, _udpClient, _webSocketClient, Host, _deviceAgent.ChannelId, Identity, _updateInterval, _timeout);

                    result = true;
                }
                else
                {
                    string channelId = await Authentication.GetChannelId();

                    if (!string.IsNullOrEmpty(channelId))
                    {
                        _deviceAgent = new DeviceAgent(_httpClient, _udpClient, _webSocketClient, Host, channelId, Identity, _updateInterval, _timeout);

                        result = true;
                    }
                    else
                    {
                        channelId = Guid.NewGuid().ToString();

                        _deviceAgent = new DeviceAgent(_httpClient, _udpClient, _webSocketClient, Host, channelId, Identity, _updateInterval, _timeout);

                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - {method}", e);
            }
            

            return result;
        }

        #endregion
    }
}

using EltraCommon.Contracts.Devices;
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

namespace EltraConnector.Agent
{
    /// <summary>
    /// End-User Connector used by ELTRA IoT clients to communicate with Cloud Service
    /// </summary>
    public class AgentConnector : IDisposable, IAgentConnector
    {
        #region Private fields

        private DeviceAgent _deviceAgent;
        private uint _updateInterval;
        private uint _timeout;
        private readonly List<(Channel, DeviceVcs)> _vcsList = new List<(Channel, DeviceVcs)>();
        private string _host;
        private UserIdentity _identity;
        private bool disposedValue;
        private AgentStatus _status;
        private Authentication _authentication;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public AgentConnector()
        {
            _updateInterval = 60;
            _timeout = 180;
        }

        #endregion

        #region Properties

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
        /// Agent status
        /// </summary>
        public AgentStatus Status
        {
            get => _status;
            private set 
            {
                if (_status != value)
                {
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
            if (Host == null)
            {
                throw new Exception("Host property not specified!");
            }

            if (Identity == null)
            {
                throw new Exception("Identity property not specified!");
            }

            Disconnect();
            
            Status = AgentStatus.Starting;

            _deviceAgent = new DeviceAgent(Host, Identity, _updateInterval, _timeout);
            
            RegisterEvents();

            bool result = await EnsureAgentReady();

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
                throw new Exception("Host property not specified!");
            }

            if (Identity == null)
            {
                throw new Exception("Identity property not specified!");
            }

            Disconnect();

            Status = AgentStatus.Starting;

            _deviceAgent = new DeviceAgent(Host, Identity, _updateInterval, _timeout);

            RegisterEvents();

            if(await EnsureAgentReady())
            {
                result = await BindChannels(deviceIdentity);
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
            }
            else if (createAccount)
            {
                if (await Authentication.SignUp(identity))
                {
                    if (await Authentication.SignIn(identity))
                    {
                        Identity = identity;
                        result = true;
                    }
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
            bool result = await Authentication.SignOut();
            
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
                throw new Exception("Host property not specified!");
            }

            if (Identity==null)
            {
                throw new Exception("Identity property not specified!");
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
                            vcs = new DeviceVcs(_deviceAgent, device);

                            vcs.DeviceChanged += (sender, args) => { DeviceDetected?.Invoke(this, new DeviceDetectedEventArgs() { Device = device }); };

                            _vcsList.Add((channel, vcs));
                        }
                        else
                        {
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
                throw new Exception("Host property not specified!");
            }

            if (Identity == null)
            {
                throw new Exception("AuthData property not specified!");
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
                throw new Exception("Host property not specified!");
            }

            if (Identity == null)
            {
                throw new Exception("AuthData property not specified!");
            }

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.UnbindChannel(channel);

                if (result)
                {
                    Status = AgentStatus.Online;
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
                if (disposing)
                {
                    if (_deviceAgent != null)
                    {
                        _deviceAgent.Dispose();
                        _deviceAgent = null;
                    }
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

        private DeviceVcs SearchDeviceVcs(EltraDevice device)
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
                    if (Channel != null)
                    {
                        Channel.Status = args.Status;
                    }
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
            bool result = false;

            if (Status == AgentStatus.Starting)
            {
                var waiter = new Stopwatch();

                waiter.Start();

                var timeout = TimeSpan.FromSeconds(_timeout);

                do
                {
                    await Task.Delay(10);

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
            return new Authentication(Host);
        }

        #endregion
    }
}

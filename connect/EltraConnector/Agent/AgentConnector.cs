using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Node;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.Agent.Events;
using EltraConnector.Events;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EltraConnector.Agent
{
    public class AgentConnector : IDisposable
    {
        #region Private fields

        private DeviceAgent _deviceAgent;
        private uint _updateInterval;
        private uint _timeout;
        private readonly List<DeviceVcs> _vcsList = new List<DeviceVcs>();
        private string _host;
        private UserData _authData;
        private bool disposedValue;

        #endregion

        #region Constructors

        public AgentConnector()
        {
            Host = "https://eltra.ch";
            AuthData = new UserData() { Login = "agent@eltra.ch", Password = "1234" };
            _updateInterval = 60;
            _timeout = 180;
        }

        #endregion

        #region Properties

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

        public UserData AuthData 
        { 
            get => _authData;
            set 
            {
                if(_authData != value)
                {
                    _authData = value;
                    OnAuthDataChanged();
                }
            } 
        }

        #endregion

        #region Events

        public event EventHandler<ChannelStatusChangedEventArgs> RemoteChannelStatusChanged;
        public event EventHandler<ChannelStatusChangedEventArgs> ChannelStatusChanged;
        public event EventHandler<AgentStatusEventArgs> AgentStatusChanged;
        public event EventHandler<DeviceFoundEventArgs> DeviceFound;

        private void OnHostChanged()
        {
            if (!string.IsNullOrEmpty(Host) && AuthData != null)
            {
                if (_deviceAgent != null)
                {
                    _deviceAgent.Dispose();
                }

                _deviceAgent = new DeviceAgent(Host, AuthData, _updateInterval, _timeout);

                RegisterEvents();
            }
        }

        private void OnAuthDataChanged()
        {
            if (!string.IsNullOrEmpty(Host) && AuthData != null)
            {
                if (_deviceAgent != null)
                {
                    _deviceAgent.Dispose();
                }

                _deviceAgent = new DeviceAgent(Host, AuthData, _updateInterval, _timeout);

                RegisterEvents();
            }
        }

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            if (_deviceAgent != null)
            {
                _deviceAgent.StatusChanged += (sender, args) => 
                {
                    AgentStatusChanged?.Invoke(this, args);
                };

                _deviceAgent.AgentChannelStatusChanged += (sender, args) =>
                {
                    ChannelStatusChanged?.Invoke(this, args);
                };

                _deviceAgent.RemoteChannelStatusChanged += (sender, args) =>
                {
                    RemoteChannelStatusChanged?.Invoke(this, args);
                };
            }
        }

        private DeviceVcs FindVcs(EltraDeviceNode device)
        {
            DeviceVcs result = null;

            if (device != null)
            {
                foreach (var vcs in _vcsList)
                {
                    var vcsDevice = vcs.Device;
                    
                    if (vcsDevice !=null &&
                        vcsDevice.NodeId == device.NodeId)
                    {
                        result = vcs;
                        break;
                    }
                }
            }

            return result;
        }

        public async Task<List<EltraDeviceNode>> GetDeviceNodes(UserData deviceAuth)
        {
            var result = new List<EltraDeviceNode>();

            _vcsList.Clear();

            if (_deviceAgent != null)
            {
                var channelDeviceNodes = await _deviceAgent.GetChannelDeviceNodes(deviceAuth);

                foreach (var channelDevice in channelDeviceNodes)
                {
                    var session = channelDevice.Channel;

                    foreach (var deviceNode in channelDevice.DeviceNodeList)
                    {
                        var device = deviceNode;

                        if (FindVcs(device) == null)
                        {
                            var vcs = new DeviceVcs(_deviceAgent, deviceNode);

                            vcs.DeviceChanged += (sender, args) => { DeviceFound?.Invoke(this, new DeviceFoundEventArgs() { Device = deviceNode }); };

                            _vcsList.Add(vcs);
                        }

                        result.Add(deviceNode);
                    }
                }
            }

            return result;
        }

        public async Task<List<EltraDeviceNodeList>> GetChannelDeviceNodes(UserData deviceAuth)
        {
            var result = new List<EltraDeviceNodeList>();

            _vcsList.Clear();

            if (_deviceAgent != null)
            {
                var channelDeviceNodeList = await _deviceAgent.GetChannelDeviceNodes(deviceAuth);

                if (channelDeviceNodeList != null)
                {
                    foreach (var deviceNodeList in channelDeviceNodeList)
                    {
                        foreach (var deviceNode in deviceNodeList.DeviceNodeList)
                        {
                            var vcs = new DeviceVcs(_deviceAgent, deviceNode);

                            vcs.DeviceChanged += (sender, args) => { DeviceFound?.Invoke(this, new DeviceFoundEventArgs() { Device = deviceNode }); };

                            _vcsList.Add(vcs);
                        }
                    }

                    result = channelDeviceNodeList;
                }
            }

            return result;
        }

        private DeviceVcs SearchDeviceVcs(EltraDevice device)
        {
            DeviceVcs result = null;

            foreach (var vcs in _vcsList)
            {
                if(vcs.Device == device)
                {
                    result = vcs;
                    break;
                }
            }

            return result;
        }

        public ParameterBase SearchParameter(EltraDevice device, ushort index, byte subIndex)
        {
            ParameterBase result = null;
            
            var vcs = SearchDeviceVcs(device);

            if(vcs!=null)
            {
                result = vcs.SearchParameter(index, subIndex);
            }

            return result;
        }

        public ParameterBase SearchParameter(EltraDevice device, string uniqueId)
        {
            ParameterBase result = null;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = vcs.SearchParameter(uniqueId);
            }

            return result;
        }

        public async Task<Parameter> GetParameter(EltraDevice device, string uniqueId)
        {
            Parameter result = null;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.GetParameter(uniqueId);
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(EltraDevice device, string uniqueId)
        {
            ParameterValue result = null;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.GetParameterValue(uniqueId);
            }

            return result;
        }

        public async Task<bool> WriteParameter(EltraDevice device, Parameter parameter)
        {
            bool result = false;

            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                result = await vcs.WriteParameter(parameter);
            }

            return result;
        }

        public void RegisterParameterUpdate(EltraDevice device, string uniqueId, ParameterUpdatePriority priority = ParameterUpdatePriority.Low)
        {
            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                vcs.RegisterParameterUpdate(uniqueId, priority);
            }
        }

        public void RegisterParameterUpdate(EltraDevice device, ushort index, byte subIndex, ParameterUpdatePriority priority = ParameterUpdatePriority.Low)
        {
            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                var parameter = vcs.SearchParameter(index, subIndex);

                if (parameter != null)
                {
                    vcs.RegisterParameterUpdate(parameter.UniqueId, priority);
                }
            }
        }

        public void UnregisterParameterUpdate(EltraDevice device, string uniqueId)
        {
            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                vcs.UnregisterParameterUpdate(uniqueId);
            }
        }

        public void UnregisterParameterUpdate(EltraDevice device, ushort index, byte subIndex)
        {
            var vcs = SearchDeviceVcs(device);

            if (vcs != null)
            {
                var parameter = vcs.SearchParameter(index, subIndex);

                if (parameter != null)
                {
                    vcs.UnregisterParameterUpdate(parameter.UniqueId);
                }
            }
        }

        public async Task<DeviceCommand> GetDeviceCommand(EltraDeviceNode device, string commandName)
        {
            DeviceCommand result = null;

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.GetDeviceCommand(device, commandName);
            }

            return result;
        }

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDeviceNode device)
        {
            var result = new List<DeviceCommand>();

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.GetDeviceCommands(device);
            }

            return result;
        }

        public async Task<List<ParameterValue>> GetParameterHistory(EltraDeviceNode device, string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();
            
            if (_deviceAgent!=null)
            {
                result = await _deviceAgent.GetParameterHistory(device, uniqueId, from, to);
            }

            return result;
        }

        public async Task<DeviceCommand> ExecuteCommand(DeviceCommand command)
        {
            DeviceCommand result = null;

            if (_deviceAgent != null)
            {
                result = await _deviceAgent.ExecuteCommand(command);
            }

            return result;
        }

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

        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using EltraConnector.Controllers;
using EltraConnector.Events;
using EltraConnector.UserAgent.Events;
using EltraCommon.Logger;

using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.Sessions;
using EltraConnector.SyncAgent;
using EltraConnector.UserAgent.Definitions;
using EltraCommon.Contracts.History;
using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.DeviceDescription.Factory;
using EltraCommon.Contracts.Parameters.Events;
using EltraCommon.ObjectDictionary.DeviceDescription;
using System.IO;

namespace EltraConnector.UserAgent
{
    internal class UserCloudAgent : IDisposable
    {
        #region Private fields

        private readonly List<DeviceCommand> _executedCommands;
        private readonly UserIdentity _identity;
        private readonly UserChannelControllerAdapter _channelAdapter;

        private Task _agentTask;
        private CancellationTokenSource _agentCancelationTokenSource;
        private Authentication _authentication;
        private Channel _channel;
        private UserIdentity _deviceIdentity;
        private AgentStatus _status = AgentStatus.Undefined;
        private ChannelStatus _channelStatus = ChannelStatus.Undefined;
        private ChannelStatus _remoteChannelStatus = ChannelStatus.Undefined;
        private string _remoteChannelId;
        private string _url;

        private ChannelHeartbeat _channelHeartbeat;
        private ExecuteCommander _executeCommander;
        private ParameterUpdateManager _parameterUpdateManager;

        #endregion

        #region Constructors

        public UserCloudAgent(string url, UserIdentity identity, uint updateInterval, uint timeout)
        {
            _status = AgentStatus.Undefined;
            _identity = identity;
            _executedCommands = new List<DeviceCommand>();
            _channelAdapter = new UserChannelControllerAdapter(url, identity, updateInterval, timeout) { UseWebSockets = true };

            Initialize(url, updateInterval, timeout);
        }

        public UserCloudAgent(string url, string uuid, UserIdentity identity, uint updateInterval, uint timeout)
        {
            _status = AgentStatus.Undefined;
            _identity = identity;
            _executedCommands = new List<DeviceCommand>();
            _channelAdapter = new UserChannelControllerAdapter(url, uuid, identity, updateInterval, timeout) { UseWebSockets = true };

            Initialize(url, updateInterval, timeout);
        }

        public UserCloudAgent(SyncCloudAgent masterAgent, EltraDevice deviceNode, uint updateInterval, uint timeout)
        {
            _status = AgentStatus.Undefined;
            _identity = masterAgent.Identity;
            _executedCommands = new List<DeviceCommand>();
            _channelAdapter = new UserChannelControllerAdapter(masterAgent.Url, deviceNode.ChannelId, masterAgent.Identity, updateInterval, timeout) { UseWebSockets = true };

            Initialize(masterAgent, deviceNode);
        }

        #endregion

        #region Events

        public event EventHandler<AgentStatusEventArgs> StatusChanged;

        public event EventHandler<ParameterValueChangedEventArgs> ParameterValueChanged;

        public event EventHandler<AgentChannelStatusChangedEventArgs> RemoteChannelStatusChanged;

        public event EventHandler<AgentChannelStatusChangedEventArgs> AgentChannelStatusChanged;

        #endregion

        #region Events handling

        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, new AgentStatusEventArgs() { Status = Status });
        }

        private void OnParameterValueChanged(object sender, ParameterValueChangedEventArgs e)
        {
            ParameterValueChanged?.Invoke(sender, e);
        }

        private void OnChannelRegistered(object sender, ChannelRegistrationEventArgs e)
        {
            _channel = e.Channel;

            if (e.Success)
            {
                _channel.Status = ChannelStatus.Online;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - OnChannelRegistered", $"Session uuid='{_channel.Id}' registration failed!");
            }
        }
        
        private void RecoverSession()
        {
            if (_deviceIdentity != null && _channelAdapter != null)
            {
                Task.Run(async () =>
                {
                    await BindChannels(_channelAdapter.ChannelId, _deviceIdentity);
                });
            }
        }

        protected virtual void OnCommandExecuted(object sender, ExecuteCommanderEventArgs e)
        {
            if (e.Command!=null)
            {
                MsgLogger.WriteDebug($"{GetType().Name} - OnCommandExecuted", $"Command executed uuid='{e.CommandUuid}', status = {e.Status}");

                AddCommandToExecuted(e.Command);
            }
            else
            {
                MsgLogger.WriteDebug($"{GetType().Name} - OnCommandExecuted", $"Command not found in pending list uuid='{e.CommandUuid}', status = {e.Status}");
            }
        }

        private void OnChannelStatusChanged()
        {
            AgentChannelStatusChanged?.Invoke(this, new AgentChannelStatusChangedEventArgs() {  Status = ChannelStatus, Id = ChannelId });

        }

        private void OnChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            if (Status == AgentStatus.Starting && e.Status == ChannelStatus.Online)
            {
                Status = AgentStatus.Started;
            }

            ChannelStatus = e.Status;
        }

        private void OnRemoteChannelStatusChanged()
        {
            RemoteChannelStatusChanged?.Invoke(this, new AgentChannelStatusChangedEventArgs() { Status = RemoteChannelStatus, Id = _remoteChannelId });
        }

        private void OnRemoteChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            _remoteChannelId = e.Id;
            RemoteChannelStatus = e.Status;

            if (e.Status == ChannelStatus.Online)
            {
                RecoverSession();
            }
        }

        private void OnSignInRequested(object sender, SignInRequestEventArgs args)
        {
            var t = Task.Run(async () => 
            {
                args.SignInResult = await SignIn(_identity); 
            });

            t.Wait();
        }

        #endregion

        #region Properties

        public Channel Channel => _channelAdapter.Channel;

        public string ChannelId => _channelAdapter.ChannelId;

        public string RemoteChannelId => _remoteChannelId;

        public AgentStatus Status 
        { 
            get => _status;
            set 
            {
                if (_status != value)
                {
                    _status = value;

                    OnStatusChanged();
                }
            }
        }

        public ChannelStatus ChannelStatus
        {
            get => _channelStatus;
            set
            {
                if(_channelStatus != value)
                {
                    _channelStatus = value;

                    OnChannelStatusChanged();
                }
            }   
        }

        public ChannelStatus RemoteChannelStatus
        {
            get => _remoteChannelStatus;
            set
            {
                if(_remoteChannelStatus != value)
                {
                    _remoteChannelStatus = value;

                    OnRemoteChannelStatusChanged();
                }
            }
        }

        #endregion

        #region Methods

        private void Initialize(string url, uint updateInterval, uint timeout)
        {
            _url = url;
            _authentication = new Authentication(url);

            _channelHeartbeat = new ChannelHeartbeat(_channelAdapter, updateInterval, timeout);
            _executeCommander = new ExecuteCommander(_channelAdapter);
            _parameterUpdateManager = new ParameterUpdateManager(_channelAdapter);

            RegisterEvents();

            _agentCancelationTokenSource = new CancellationTokenSource();

            _agentTask = Task.Run(() => Run(_agentCancelationTokenSource.Token));
        }

        private void Initialize(SyncCloudAgent agent, EltraDevice node)
        {
            _url = agent.Url;
            _authentication = new Authentication(agent.Url);

            _executeCommander = new ExecuteCommander(_channelAdapter, node.NodeId);
            _parameterUpdateManager = new ParameterUpdateManager(_channelAdapter, node.NodeId);

            RegisterEvents();

            _agentCancelationTokenSource = new CancellationTokenSource();

            _agentTask = Task.Run(() => RunMaster(_agentCancelationTokenSource.Token));
        }

        public void Release()
        {
            _agentCancelationTokenSource?.Cancel();      
            _agentTask?.Wait();
        }

        private void RegisterParameterUpdateManagerEvents()
        {
            UnregisterParameterUpdateManagerEvents();

            _parameterUpdateManager.ParameterValueChanged += OnParameterValueChanged;
        }
        
        private void UnregisterParameterUpdateManagerEvents()
        {
            _parameterUpdateManager.ParameterValueChanged -= OnParameterValueChanged;
        }
        
        private async Task<bool> RegisterChannel(CancellationToken token)
        {
            const int minWaitTime = 100;
            bool result = false;

            while (!token.IsCancellationRequested)
            {
                if (await RegisterSession())
                {
                    result = true;
                    break;
                }

                await Task.Delay(minWaitTime);
            }

            return result;
        }

        private bool StartChannel(CancellationToken token)
        {
            bool result = false;

            if (!token.IsCancellationRequested)
            {
                _channelHeartbeat?.Start();
                _executeCommander?.Start();
                _parameterUpdateManager?.Start();

                RegisterParameterUpdateManagerEvents();

                result = true;
            }

            return result;
        }

        private async Task StopChannel()
        {
            _executeCommander?.Stop();
           
            await UnregisterSession();

            _channelHeartbeat?.Stop();

            UnregisterParameterUpdateManagerEvents();

            _parameterUpdateManager?.Stop();
        }

        private async Task Run(CancellationToken token)
        {
            Status = AgentStatus.Starting;

            if(await RegisterChannel(token))
            {
                if (StartChannel(token))
                {
                    await WaitForRequests(token);

                    await StopChannel();
                }
            }

            MsgLogger.WriteLine($"Sync agent working thread finished successfully!");
            
            Status = AgentStatus.Stopped;
        }

        private async Task RunMaster(CancellationToken token)
        {
            Status = AgentStatus.Starting;

            if (StartChannel(token))
            {
                await WaitForRequests(token);

                await StopChannel();
            }
            
            MsgLogger.WriteLine($"Sync agent working thread finished successfully!");

            Status = AgentStatus.Stopped;
        }

        private static async Task WaitForRequests(CancellationToken token)
        {
            const int minWaitTime = 10;

            do
            {
                await Task.Delay(minWaitTime);
            } while (!token.IsCancellationRequested);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);            
        }

        protected virtual void Dispose(bool finalize)
        {
            if (finalize)
            {
                Release();
            }
        }

        private async Task<bool> UpdateSession()
        {
            return await _channelAdapter.Update();
        }

        private async Task<bool> RegisterSession()
        {
            bool result = false;

            if (await SignIn(_identity))
            {
                result = await UpdateSession();
            }
            else if (await SignUp(_identity))
            {
                if (await SignIn(_identity))
                {
                    result = await UpdateSession();
                }
            }

            return result;
        }

        private async Task<bool> UnregisterSession()
        {
            bool result = false;

            if (_channelHeartbeat != null)
            {
                result = await _channelAdapter.UnregisterChannel();
            }

            return result;
        }

        public async Task<bool> BindChannels(string uuid, UserIdentity identity)
        {
            _deviceIdentity = identity;

            var result = await _channelAdapter.BindChannels(uuid, identity);

            return result;
        }

        public async Task<bool> BindChannels(UserIdentity identity)
        {
            _deviceIdentity = identity;

            var result = await _channelAdapter.BindChannels(_channelAdapter.ChannelId, identity);

            return result;
        }

        public async Task<bool> UnbindChannel(Channel channel)
        {
            var result = await _channelAdapter.UnbindChannel(_channelAdapter.ChannelId, channel.Id);

            return result;
        }

        private void RegisterEvents()
        {
            if (_channelHeartbeat != null)
            {
                _channelHeartbeat.StatusChanged += OnChannelStatusChanged;
                _channelHeartbeat.SignInRequested += OnSignInRequested;
            }

            _parameterUpdateManager.SignInRequested += OnSignInRequested;

            _channelAdapter.ChannelRegistered += OnChannelRegistered;
            
            _executeCommander.CommandExecuted += OnCommandExecuted;
            _executeCommander.RemoteChannelStatusChanged += OnRemoteChannelStatusChanged;
            _executeCommander.SignInRequested += OnSignInRequested;
        }

        private async Task<List<EltraDevice>> GetDeviceNodes(Channel channel)
        {
            return await _channelAdapter.GetDeviceNodes(channel);
        }

        protected virtual async Task UpdateDeviceDescriptionFile(EltraDevice device)
        {
            if (device != null && device.ObjectDictionary == null)
            {
                var deviceDescriptionFile = DeviceDescriptionFactory.CreateDeviceDescriptionFile(device);

                if (deviceDescriptionFile != null)
                {
                    deviceDescriptionFile.Url = _url;
                    
                    if (!await device.ReadDeviceDescriptionFile(deviceDescriptionFile))
                    {
                        MsgLogger.WriteError($"{GetType().Name} - GetChannels", "read device description file failed!");
                    }
                }
                else
                {
                    device.Status = DeviceStatus.Ready;
                }
            }
        }

        public async Task<List<Channel>> GetChannels()
        {
            var channels = await _channelAdapter.GetChannels();

            foreach (var channel in channels)
            {
                foreach (var device in channel.Devices)
                {
                    await UpdateDeviceDescriptionFile(device);
                }
            }

            return channels;
        }
        
        public virtual async Task<DeviceCommand> GetDeviceCommand(EltraDevice device, string commandName)
        {
            var result = await _channelAdapter.GetDeviceCommand(device, commandName);
            
            return result;
        }

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDevice device)
        {
            return await _channelAdapter.GetDeviceCommands(device);
        }

        public async Task<bool> PushCommand(DeviceCommand command, ExecCommandStatus status)
        {
            command.Id = Guid.NewGuid().ToString();

            return await _channelAdapter.PushCommand(command, ChannelId, status);
        }

        private void AddCommandToExecuted(DeviceCommand command)
        {
            lock (this)
            {
                _executedCommands.Add(command);
            }
        }

        private DeviceCommand FindExecutedCommand(DeviceCommand deviceCommand)
        {
            DeviceCommand result = null;

            lock (this)
            {
                foreach (var command in _executedCommands)
                {
                    if (deviceCommand.Id == command.Id)
                    {
                        result = command;
                        break;
                    }
                }
            }

            return result;
        }

        private void RemoveCommandFromExecuted(DeviceCommand deviceCommand)
        {
            lock (this)
            {
                if (_executedCommands.Contains(deviceCommand))
                {
                    _executedCommands.Remove(deviceCommand);
                }
            }
        }

        public async Task<DeviceCommand> ExecuteCommand(DeviceCommand command)
        {
            const int minWaitTime = 10;
            const int maxWaitTime = 10000;
            DeviceCommand result = null;

            if (await PushCommand(command, ExecCommandStatus.Waiting))
            {
                _executeCommander.FollowCommand(command);

                var waitWatch = new Stopwatch();
                
                waitWatch.Start();
                
                do
                {
                    await Task.Delay(minWaitTime);

                } while (command.Status == ExecCommandStatus.Waiting && waitWatch.ElapsedMilliseconds < maxWaitTime);

                if(command.Status != ExecCommandStatus.Waiting)
                {
                    result = FindExecutedCommand(command);
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand", $"command '{command.Id}' timeout, status = {command.Status}");
                }

                if(result!=null)
                { 
                    RemoveCommandFromExecuted(result);
                }
            }
            
            return result;
        }

        public async Task<bool> ExecuteCommandAsync(DeviceCommand command)
        {
            bool result = false;

            if (await PushCommand(command, ExecCommandStatus.Waiting))
            {
                _executeCommander.FollowCommand(command);

                result = true;
            }

            return result;
        }

        private DeviceDescriptionPayload GetDeviceDescriptionFromCache(DeviceDescriptionIdentity identity, DeviceVersion deviceVersion)
        {
            DeviceDescriptionPayload result = null;
            var tempPath = Path.GetTempPath();

            if (identity != null && deviceVersion != null)
            {
                try
                {
                    var localPath = Path.Combine(tempPath, identity.Content + ".xdd");

                    if (File.Exists(localPath))
                    {
                        var xddContent = File.ReadAllText(localPath);
                        var fi = new FileInfo(localPath);

                        var payload = new DeviceDescriptionPayload()
                        {
                            ChannelId = _channelAdapter.ChannelId,
                            Encoding = identity.Encoding,
                            Content = xddContent,
                            Version = deviceVersion,
                            Modified = fi.LastWriteTime
                        };

                        if(payload.HashCode == identity.Content)
                        {
                            result = payload;
                        }
                    }
                }
                catch(Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - GetDeviceDescriptionFromCache", e);
                }
            }

            return result;
        }

        private void CacheDeviceDescriptionFile(DeviceDescriptionPayload payload)
        {
            try
            {
                if (payload != null)
                {
                    var tempPath = Path.GetTempPath();
                    var localPath = Path.Combine(tempPath, payload.HashCode + ".xdd");

                    File.WriteAllText(localPath, payload.Content);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CacheDeviceDescriptionFile", e);
            }
        }

        public async Task<DeviceDescriptionPayload> DownloadDeviceDescription(DeviceVersion deviceVersion)
        {
            DeviceDescriptionPayload result = null;

            if (_channelAdapter != null)
            {
                var channelId = _channelAdapter.ChannelId;

                var identity = await _channelAdapter.GetDeviceDescriptionIdentity(channelId, deviceVersion);

                if (identity == null)
                {
                    result = await _channelAdapter.DownloadDeviceDescription(channelId, deviceVersion);

                    if (result != null)
                    {
                        CacheDeviceDescriptionFile(result);
                    }
                }
                else
                {
                    var cachedDeviceDescriptionPayload = GetDeviceDescriptionFromCache(identity, deviceVersion);

                    if (cachedDeviceDescriptionPayload != null)
                    {
                        result = cachedDeviceDescriptionPayload;
                    }
                    else
                    {
                        result = await _channelAdapter.DownloadDeviceDescription(channelId, deviceVersion);

                        if (result != null)
                        {
                            CacheDeviceDescriptionFile(result);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<Parameter> GetParameter(string channelId, int nodeId, ushort index, byte subIndex)
        {
            return await _channelAdapter.GetParameter(channelId, nodeId, index, subIndex);
        }

        public async Task<ParameterValue> GetParameterValue(string channelId, int nodeId, ushort index, byte subIndex)
        {
            return await _channelAdapter.GetParameterValue(channelId, nodeId, index, subIndex);
        }

        public async Task<List<ParameterValue>> GetParameterValueHistory(string channelId, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            return await _channelAdapter.GetParameterHistory(channelId, nodeId, uniqueId, from, to);
        }

        public async Task<ParameterValueHistoryStatistics> GetParameterValueHistoryStatistics(string channelId, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            return await _channelAdapter.GetParameterHistoryStatistics(channelId, nodeId, uniqueId, from, to);
        }

        public async Task<bool> SignOut()
        {
            return await _authentication.SignOut();
        }

        public async Task<bool> SignUp(UserIdentity identity)
        {
            return await _authentication.SignUp(identity);
        }

        public async Task<bool> SignIn(UserIdentity identity)
        {
            return await _authentication.SignIn(identity);
        }



        #endregion
    }
}

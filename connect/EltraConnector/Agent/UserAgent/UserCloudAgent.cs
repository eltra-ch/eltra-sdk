﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EltraConnector.Events;
using EltraConnector.UserAgent.Events;
using EltraCommon.Logger;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.SyncAgent;
using EltraConnector.UserAgent.Definitions;
using EltraCommon.Contracts.History;
using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.DeviceDescription.Factory;
using EltraCommon.Contracts.Parameters.Events;
using EltraCommon.ObjectDictionary.DeviceDescription;
using System.IO;
using EltraConnector.Channels.Events;
using EltraConnector.Channels;
using EltraCommon.Transport;
using EltraConnector.Agent.Controllers;
using EltraConnector.Agent.Controllers.Commands;
using EltraConnector.Agent.Parameters;
using EltraConnector.Transport;
using EltraConnector.Agent.Controllers.Heartbeat;
using EltraConnector.Agent.Controllers.Parameters;
using EltraConnector.Agent.UserAgent.Events;
using EltraConnector.Transport.Ws;
using EltraConnector.Transport.Udp;

namespace EltraConnector.UserAgent
{
    internal class UserCloudAgent : IDisposable
    {
        #region Private fields

        private readonly List<DeviceCommand> _executedCommands;
        private readonly UserIdentity _identity;
        private readonly SlaveChannelControllerAdapter _channelAdapter;
        private readonly uint _timeout;
        private readonly object _lock = new object();
        private readonly IHttpClient _httpClient;

        private Task _agentTask;
        private CancellationTokenSource _agentCancelationTokenSource;
        private Authentication _authentication;
        private Channel _channel;
        private UserIdentity _deviceIdentity;
        private AgentStatus _status;
        private ChannelStatus _channelStatus = ChannelStatus.Undefined;
        private ChannelStatus _remoteChannelStatus = ChannelStatus.Undefined;
        private string _remoteChannelId;
        private string _url;
        private ChannelHeartbeat _channelHeartbeat;
        private SlaveExecuteCommander _executeCommander;
        private ParameterUpdateManager _parameterUpdateManager;

        #endregion

        #region Constructors

        public UserCloudAgent(IHttpClient httpClient, IUdpClient udpClient, IWebSocketClient webSocketClient, string url, string uuid, UserIdentity identity, uint updateInterval, uint timeout)
        {
            _httpClient = httpClient;
            _timeout = timeout;
            _status = AgentStatus.Undefined;
            _identity = identity;
            _channel = new Channel() { Status = ChannelStatus.Offline };
            _executedCommands = new List<DeviceCommand>();

            _channelAdapter = new SlaveChannelControllerAdapter(httpClient, udpClient, url, uuid, identity, updateInterval, timeout) { ConnectionManager = new ConnectionManager(httpClient, udpClient, webSocketClient) { HostUrl = url } };

            Initialize(url, updateInterval);
        }

        public UserCloudAgent(IHttpClient httpClient, IUdpClient udpClient, SyncCloudAgent masterAgent, EltraDevice deviceNode, uint updateInterval, uint timeout)
        {
            _httpClient = httpClient;
            _status = AgentStatus.Undefined;
            _identity = masterAgent.Identity;
            _channel = new Channel() { Status = ChannelStatus.Offline };
            _executedCommands = new List<DeviceCommand>();

            _channelAdapter = new MasterSlaveChannelControllerAdapter(httpClient, udpClient, masterAgent, deviceNode, updateInterval, timeout)
            {
                ConnectionManager = masterAgent.ConnectionManager 
            };

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
            MsgLogger.WriteLine($"{GetType().Name} - OnStatusChanged", $"agent status changed to {Status}");

            StatusChanged?.Invoke(this, new AgentStatusEventArgs() { Status = Status });
        }

        private void OnParameterValueChanged(object sender, ParameterValueChangedEventArgs e)
        {
            ParameterValueChanged?.Invoke(sender, e);
        }

        private void OnChannelRegistered(object sender, ChannelRegistrationEventArgs e)
        {
            _channel = e.Channel;

            _channel.StatusChanged += (a, f) => {

                if (Status == AgentStatus.Starting && f.Status == ChannelStatus.Online)
                {
                    Status = AgentStatus.Online;
                }
                else if (Status == AgentStatus.Online && f.Status == ChannelStatus.Offline)
                {
                    Status = AgentStatus.Offline;
                }

                ChannelStatus = f.Status;
            };

            if (e.Success)
            {
                _channel.Status = ChannelStatus.Online;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - OnChannelRegistered", $"channel id='{_channel.Id}' registration failed!");
            }
        }
        
        private void RecoverChannel()
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

        private void OnHeartbeatChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            if (Status == AgentStatus.Starting && e.Status == ChannelStatus.Online)
            {
                Status = AgentStatus.Online;
            }
            else if (Status == AgentStatus.Online && e.Status == ChannelStatus.Offline)
            {
                Status = AgentStatus.Offline;
            }

            MsgLogger.WriteDebug($"{GetType().Name} - OnHeartbeatChannelStatusChanged", $"AgentChannelStatus = {e.Status}, Agent Status = {Status}");

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
                RecoverChannel();
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

        public CloudControllerAdapter Adapter => _channelAdapter;

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

        private void Initialize(string url, uint updateInterval)
        {
            const string method = "Initialize";

            _url = url;
            _authentication = new Authentication(_httpClient, url);

            _executeCommander = new SlaveExecuteCommander(_channelAdapter);
            _channelHeartbeat = new SlaveChannelHeartbeat(_channelAdapter, _executeCommander, _timeout, updateInterval);            
            _parameterUpdateManager = new SlaveParameterUpdateManager(_channelAdapter, _executeCommander);

            RegisterEvents();

            _agentCancelationTokenSource = new CancellationTokenSource();

            if(!StartWorkingTask())
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", "Start working task failed!");
            }
        }

        private void Initialize(SyncCloudAgent agent, EltraDevice node)
        {
            _url = agent.Url;
            _authentication = new Authentication(_httpClient, agent.Url);

            _executeCommander = new SlaveExecuteCommander(_channelAdapter, node.NodeId);
            _parameterUpdateManager = new SlaveParameterUpdateManager(_channelAdapter, node.NodeId, _executeCommander);

            RegisterEvents();

            _agentCancelationTokenSource = new CancellationTokenSource();

            StartWorkingTask();
        }

        private bool StartWorkingTask()
        {
            bool result = false;

            if(Status == AgentStatus.Stopped)
            {
                Status = AgentStatus.Undefined;
            }

            _agentTask = Task.Run(async () => await Run(_agentCancelationTokenSource.Token));

            const int minWaitTime = 10;
            
            int startupTimeout = (int)TimeSpan.FromSeconds(_timeout).TotalMilliseconds;
            var stopWatch = new Stopwatch();
            
            stopWatch.Start();

            StatusChanged += (o, e) =>
            {
                if (e.Status == AgentStatus.Started)
                {
                    result = true;
                }
            };

            long elapsedMiliseconds = stopWatch.ElapsedMilliseconds;
            while (!result && elapsedMiliseconds < startupTimeout && Status != AgentStatus.Stopped)
            {
                Thread.Sleep(minWaitTime);

                elapsedMiliseconds = stopWatch.ElapsedMilliseconds;
            }

            return result;
        }

        public void Release()
        {
            int releaseTimeout = (int)TimeSpan.FromSeconds(_timeout).TotalMilliseconds;

            _agentCancelationTokenSource?.Cancel();
            
            _agentTask?.Wait(releaseTimeout);

            Status = AgentStatus.Undefined;
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
        
        private bool StartChannel(CancellationToken token)
        {
            bool result = false;

            if (!token.IsCancellationRequested)
            {
                var stopWatch = new Stopwatch();

                stopWatch.Start();

                Task parameterUpdateTask = null;
                var tasks = new List<Task>();

                if (_channelHeartbeat != null)
                {
                    _channelHeartbeat.Start();

                    parameterUpdateTask = Task.Run(() =>
                    {
                        _parameterUpdateManager?.Start();
                    });

                    tasks.Add(parameterUpdateTask);
                }
                else
                {
                    _parameterUpdateManager.Start();
                }

                var executeCommanderTask = Task.Run(() =>
                {
                    _executeCommander?.Start();
                });

                tasks.Add(executeCommanderTask);

                RegisterParameterUpdateManagerEvents();

                Task.WaitAll(tasks.ToArray());

                stopWatch.Stop();

                MsgLogger.WriteFlow($"{GetType().Name} - StartChannel", $"channels started, startup time = {stopWatch.ElapsedMilliseconds} ms");

                if (_parameterUpdateManager != null)
                {
                    result = _parameterUpdateManager.Status == WsChannelStatus.Started;
                }

                if (result)
                {
                    if (_executeCommander != null)
                    {
                        result = _executeCommander.Status == WsChannelStatus.Started;
                    }
                }
                else
                {
                    if (_parameterUpdateManager != null)
                    {
                        _parameterUpdateManager.Stop();
                    }
                }

                if (result)
                {
                    if (_channelHeartbeat != null)
                    {
                        result = _channelHeartbeat.Status == WsChannelStatus.Started;

                        if(!result)
                        {
                            _channelHeartbeat.Stop();
                        }
                    }
                }
                else
                {
                    if (_executeCommander != null)
                    {
                        _executeCommander.Stop();
                    }
                }
            }

            return result;
        }

        public async Task<bool> SetParameterValue(EltraDevice device, ushort index, byte subIndex, ParameterValue parameterValue)
        {
            bool result = false;

            if (_channelAdapter != null)
            {
                result = await _channelAdapter.SetParameterValue(device, index, subIndex, parameterValue);
            }

            return result;
        }

        private async Task StopChannel()
        {
            UnregisterParameterUpdateManagerEvents();

            MsgLogger.WriteDebug($"{GetType().Name} - StopChannel", $"stop heartbeat channel");

            _channelHeartbeat?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - StopChannel", $"stop execute commander channel");

            _executeCommander?.Stop();

            MsgLogger.WriteDebug($"{GetType().Name} - StopChannel", $"stop parameter update channel");

            _parameterUpdateManager?.Stop();

            await UnregisterChannel();
        }

        private async Task Run(CancellationToken token)
        {
            Status = AgentStatus.Starting;

            if(await RegisterChannel(token))
            {
                Status = AgentStatus.Registered;

                if (!StartChannel(token))
                {
                    MsgLogger.WriteError($"{GetType().Name} - Run", $"starting channel failed");
                }
                else
                {
                    Status = AgentStatus.Started;
                }

                await WaitForRequests(token);

                await StopChannel();
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - Run", $"channel registration failed!");
            }

            MsgLogger.WriteLine($"{GetType().Name} - Run", $"UserCloud agent working thread finished successfully!");
            
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

        private async Task<bool> UpdateChannel()
        {
            return await _channelAdapter.Update();
        }

        private async Task<bool> RegisterChannel(CancellationToken token)
        {
            const int minWaitTime = 300;
            bool result = false;

            while (!token.IsCancellationRequested)
            {
                if (await RegisterChannel())
                {
                    result = true;
                    break;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - RegisterChannel", "Channel register failed!");
                }

                await Task.Delay(minWaitTime);
            }

            return result;
        }

        private async Task<bool> RegisterChannel()
        {
            bool result = false;

            if (await SignIn(_identity))
            {
                result = await UpdateChannel();
            }
            else if (await SignUp(_identity) && await SignIn(_identity))
            {
                result = await UpdateChannel();
            }

            return result;
        }

        private async Task<bool> UnregisterChannel()
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

            if(result)
            {
                Status = AgentStatus.Bound;
            }

            return result;
        }

        public async Task<bool> BindChannels(UserIdentity identity)
        {
            _deviceIdentity = identity;

            var result = await _channelAdapter.BindChannels(_channelAdapter.ChannelId, identity);

            if (result)
            {
                Status = AgentStatus.Bound;
            }

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
                _channelHeartbeat.ChannelStatusChanged += OnHeartbeatChannelStatusChanged;
                _channelHeartbeat.SignInRequested += OnSignInRequested;
                _channelHeartbeat.StatusChanged += OnHeartbeatStatusChanged;
            }

            if (_parameterUpdateManager != null)
            {
                _parameterUpdateManager.SignInRequested += OnSignInRequested;
                _parameterUpdateManager.StatusChanged += OnParameterUpdateStatusChanged;
            }

            if (_channelAdapter != null)
            {
                _channelAdapter.ChannelRegistered += OnChannelRegistered;
            }

            if (_executeCommander != null)
            {
                _executeCommander.CommandExecuted += OnCommandExecuted;
                _executeCommander.RemoteChannelStatusChanged += OnRemoteChannelStatusChanged;
                _executeCommander.SignInRequested += OnSignInRequested;
                _executeCommander.StatusChanged += OnExecuteCommanderStatusChanged;
            }
        }

        private void OnHeartbeatStatusChanged(object sender, WsChannelStatusEventArgs e)
        {
            if (_executeCommander != null &&
                _executeCommander.Status == WsChannelStatus.Started &&
                _parameterUpdateManager.Status == WsChannelStatus.Started && 
                e.Status == WsChannelStatus.Started)
            {
                Status = AgentStatus.Started;
            }
        }

        private void OnParameterUpdateStatusChanged(object sender, WsChannelStatusEventArgs e)
        {
            if (_channelHeartbeat != null && 
                _channelHeartbeat.Status == WsChannelStatus.Started && 
                _executeCommander.Status == WsChannelStatus.Started && 
                e.Status == WsChannelStatus.Started)
            {
                Status = AgentStatus.Started;
            }
        }

        private void OnExecuteCommanderStatusChanged(object sender, WsChannelStatusEventArgs e)
        {
            if (_channelHeartbeat != null &&
                _channelHeartbeat.Status == WsChannelStatus.Started &&
                _parameterUpdateManager.Status == WsChannelStatus.Started && 
                e.Status == WsChannelStatus.Started)
            {
                Status = AgentStatus.Started;
            }
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
                    device.ChannelId = channel.Id;
                    device.ChannelLocalHost = channel.LocalHost;

                    if(channel.Status == ChannelStatus.Online)
                    {
                        await UpdateDeviceDescriptionFile(device);
                    }
                    else
                    {
                        _ = Task.Run(async () => { await UpdateDeviceDescriptionFile(device); });
                    }
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
            lock (_lock)
            {
                _executedCommands.Add(command);
            }
        }

        private DeviceCommand FindExecutedCommand(DeviceCommand deviceCommand)
        {
            DeviceCommand result = null;

            lock (_lock)
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
            lock (_lock)
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
            DeviceCommand result = null;

            if (await PushCommand(command, ExecCommandStatus.Waiting))
            {
                _executeCommander.FollowCommand(command);

                var waitWatch = new Stopwatch();
                
                waitWatch.Start();
                
                do
                {
                    await Task.Delay(minWaitTime);

                } while (command.Status == ExecCommandStatus.Waiting && waitWatch.ElapsedMilliseconds < command.Timeout);

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
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand", $"push command '{command.Id}' failed!");
            }

            return result;
        }

        public async Task<bool> ExecuteCommandAsync(DeviceCommand command)
        {
            bool result = false;
            const string methodName = "ExecuteCommandAsync";

            if (await PushCommand(command, ExecCommandStatus.Waiting))
            {
                _executeCommander.FollowCommand(command);

                result = true;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"push command '{command.Id}' failed!");
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

                    if (!File.Exists(localPath))
                    {
                        File.WriteAllText(localPath, payload.Content);
                    }
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

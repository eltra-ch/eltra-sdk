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
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraConnector.Sessions;
using EltraConnector.SyncAgent;
using EltraConnector.UserAgent.Definitions;
using EltraCommon.Contracts.Node;

namespace EltraConnector.UserAgent
{
    internal class UserCloudAgent : IDisposable
    {
        #region Private fields

        private readonly List<DeviceCommand> _executedCommands;
        private readonly UserData _authData;
        private readonly UserChannelControllerAdapter _channelAdapter;

        private Task _agentTask;
        private CancellationTokenSource _agentCancelationTokenSource;
        private ChannelHeartbeat _sessionUpdater;
        private ExecuteCommander _executeCommander;
        private ParameterUpdateManager _parameterUpdateManager;        
        private Authentication _authentication;
        private Channel _channel;
        private UserData _deviceAuthData;

        #endregion

        #region Constructors

        public UserCloudAgent(string url, UserData authData, uint updateInterval, uint timeout)
        {
            _authData = authData;
            _executedCommands = new List<DeviceCommand>();
            _channelAdapter = new UserChannelControllerAdapter(url, authData, updateInterval, timeout) { UseWebSockets = true };

            Initialize(url, updateInterval, timeout, true);
        }

        public UserCloudAgent(string url, string uuid, UserData authData, uint updateInterval, uint timeout)
        {
            _authData = authData;
            _executedCommands = new List<DeviceCommand>();
            _channelAdapter = new UserChannelControllerAdapter(url, uuid, authData, updateInterval, timeout) { UseWebSockets = true };

            Initialize(url, updateInterval, timeout, true);
        }

        public UserCloudAgent(SyncCloudAgent masterAgent, EltraDeviceNode device, uint updateInterval, uint timeout)
        {
            _authData = masterAgent.AuthData;
            _executedCommands = new List<DeviceCommand>();
            _channelAdapter = new UserChannelControllerAdapter(masterAgent.Url, device.ChannelId, masterAgent.AuthData, updateInterval, timeout) { UseWebSockets = true };

            Initialize(masterAgent.Url, updateInterval, timeout, false);
        }

        #endregion

        #region Events

        public event EventHandler<ParameterChangedEventArgs> ParameterChanged;

        public event EventHandler<ChannelStatusChangedEventArgs> RemoteChannelStatusChanged;

        #endregion

        #region Events handling

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            ParameterChanged?.Invoke(sender, e);
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
        private void OnRemoteChannelStatusChanged(object sender, ChannelStatusChangedEventArgs e)
        {
            if(e.Status == ChannelStatus.Online)
            {
                RecoverSession();
            }

            RemoteChannelStatusChanged?.Invoke(sender, e);
        }

        private void RecoverSession()
        {
            if (_deviceAuthData != null && _channelAdapter != null)
            {
                Task.Run(async () =>
                {
                    await GetChannels(_channelAdapter.Id, _deviceAuthData);
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

        private void OnChannelStatusChanged(object sender, ChannelStatusChangedEventArgs e)
        {
            if(State == UserCloudAgentState.Starting && e.Status == ChannelStatus.Online)
            {
                State = UserCloudAgentState.Started;
            }    
        }

        #endregion

        #region Properties

        public string Uuid => _channelAdapter.Id;

        public UserCloudAgentState State { get; set; } = UserCloudAgentState.Undefined;

        #endregion

        #region Methods

        private void Initialize(string url, uint updateInterval, uint timeout, bool useSessionUpdater)
        {
            _authentication = new Authentication(url);

            if (useSessionUpdater)
            {
                _sessionUpdater = new ChannelHeartbeat(_channelAdapter, updateInterval, timeout);

                _sessionUpdater.StatusChanged += OnChannelStatusChanged;
            }

            _executeCommander = new ExecuteCommander(_channelAdapter);
            _parameterUpdateManager = new ParameterUpdateManager(_channelAdapter);

            RegisterEvents();

            _agentCancelationTokenSource = new CancellationTokenSource();

            _agentTask = Task.Run(() => Run(_agentCancelationTokenSource.Token));
        }

        public void Release()
        {
            _agentCancelationTokenSource?.Cancel();      
            _agentTask?.Wait();
        }

        private void RegisterParameterUpdateManagerEvents()
        {
            UnregisterParameterUpdateManagerEvents();

            _parameterUpdateManager.ParameterChanged += OnParameterChanged;
        }
        
        private void UnregisterParameterUpdateManagerEvents()
        {
            _parameterUpdateManager.ParameterChanged -= OnParameterChanged;
        }
        
        private async Task<bool> RegisterSession(CancellationToken token)
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

        private bool StartSession(CancellationToken token)
        {
            bool result = false;

            if (!token.IsCancellationRequested)
            {
                _sessionUpdater?.Start();
                _executeCommander?.Start();
                _parameterUpdateManager?.Start();

                RegisterParameterUpdateManagerEvents();

                result = true;
            }

            return result;
        }

        private async Task StopSession()
        {
            _executeCommander?.Stop();
           
            await UnregisterSession();

            _sessionUpdater?.Stop();

            UnregisterParameterUpdateManagerEvents();

            _parameterUpdateManager?.Stop();
        }

        private async Task Run(CancellationToken token)
        {
            State = UserCloudAgentState.Starting;

            if(await RegisterSession(token))
            {
                if (StartSession(token))
                {
                    await SessionLoop(token);

                    await StopSession();
                }
            }

            MsgLogger.WriteLine($"Sync agent working thread finished successfully!");
            
            State = UserCloudAgentState.Stopped;
        }

        private static async Task SessionLoop(CancellationToken token)
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

            if (await SignIn(_authData))
            {
                result = await UpdateSession();
            }
            else if (await SignUp(_authData))
            {
                if (await SignIn(_authData))
                {
                    result = await UpdateSession();
                }
            }

            return result;
        }

        private async Task<bool> UnregisterSession()
        {
            bool result = false;

            if (_sessionUpdater != null)
            {
                result = await _channelAdapter.UnregisterChannel();
            }

            return result;
        }

        public async Task<List<Channel>> GetChannels(string uuid, UserData authData)
        {
            _deviceAuthData = authData;

            var result = await _channelAdapter.GetChannels(uuid, authData);

            return result;
        }

        private void RegisterEvents()
        {
            _channelAdapter.SessionRegistered += OnChannelRegistered;

            _executeCommander.CommandExecuted += OnCommandExecuted;
            _executeCommander.RemoteChannelStatusChanged += OnRemoteChannelStatusChanged;
        }

        private async Task<List<EltraDeviceNode>> GetDeviceNodes(Channel channel, UserData authData)
        {
            await EnsureAgentReady();

            return await _channelAdapter.GetDeviceNodes(channel, authData);
        }

        private async Task<bool> EnsureAgentReady()
        {
            bool result = false;

            if (State == UserCloudAgentState.Starting)
            {
                do
                {
                    await Task.Delay(10);
                } while (State != UserCloudAgentState.Started);

                result = true;
            }
            else if(State == UserCloudAgentState.Started)
            {
                result = true;
            }

            return result;
        }

        public async Task<List<EltraDeviceNodeList>> GetChannelDeviceNodes(UserData authData)
        {
            var result = new List<EltraDeviceNodeList>();

            if (await EnsureAgentReady())
            {
                var channels = await GetChannels(_channelAdapter.Id, authData);

                if (channels != null)
                {
                    foreach (var channel in channels)
                    {
                        var deviceNodeList = new EltraDeviceNodeList() { Channel = channel };

                        var deviceNodes = await GetDeviceNodes(channel, authData);

                        if (deviceNodes != null)
                        {
                            foreach (var deviceNode in deviceNodes)
                            {
                                deviceNodeList.AddDevice(deviceNode);
                            }
                        }

                        result.Add(deviceNodeList);
                    }
                }
            }

            return result;
        }
        
        public virtual async Task<DeviceCommand> GetDeviceCommand(EltraDeviceNode device, string commandName)
        {
            await EnsureAgentReady();

            var result = await _channelAdapter.GetDeviceCommand(device, commandName);
            
            return result;
        }

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDeviceNode device)
        {
            await EnsureAgentReady();

            return await _channelAdapter.GetDeviceCommands(device);
        }

        public async Task<bool> PushCommand(DeviceCommand command, ExecCommandStatus status)
        {
            await EnsureAgentReady();

            command.Id = Guid.NewGuid().ToString();

            return await _channelAdapter.PushCommand(command, Uuid, status);
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

            await EnsureAgentReady();

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

            await EnsureAgentReady();

            if (await PushCommand(command, ExecCommandStatus.Waiting))
            {
                _executeCommander.FollowCommand(command);

                result = true;
            }

            return result;
        }

        public async Task<Parameter> GetParameter(string sessionUuid, int nodeId, ushort index, byte subIndex)
        {
            await EnsureAgentReady();

            return await _channelAdapter.GetParameter(sessionUuid, nodeId, index, subIndex);
        }

        public async Task<ParameterValue> GetParameterValue(string sessionUuid, int nodeId, ushort index, byte subIndex)
        {
            await EnsureAgentReady();

            return await _channelAdapter.GetParameterValue(sessionUuid, nodeId, index, subIndex);
        }

        public async Task<List<ParameterValue>> GetParameterHistory(string sessionUuid, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            await EnsureAgentReady();

            return await _channelAdapter.GetParameterHistory(sessionUuid, nodeId, uniqueId, from, to);
        }

        public async Task<bool> SignOut()
        {
            return await _authentication.SignOut();
        }

        public async Task<bool> SignUp(UserData authData)
        {
            return await _authentication.SignUp(authData);
        }

        public async Task<bool> SignIn(UserData authData)
        {
            return await _authentication.SignIn(authData);
        }

        #endregion
    }
}

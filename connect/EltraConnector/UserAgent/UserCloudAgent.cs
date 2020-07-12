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
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Sessions;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.Contracts.Parameters;
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
        private readonly UserAuthData _authData;
        private readonly UserSessionControllerAdapter _sessionAdapter;

        private Task _agentTask;
        private CancellationTokenSource _agentCancelationTokenSource;
        private SessionUpdater _sessionUpdater;
        private ExecuteCommander _executeCommander;
        private ParameterUpdateManager _parameterUpdateManager;        
        private Authentication _authentication;
        private Session _session;
        private UserAuthData _deviceAuthData;

        #endregion

        #region Constructors

        public UserCloudAgent(string url, UserAuthData authData, uint updateInterval, uint timeout)
        {
            _authData = authData;
            _executedCommands = new List<DeviceCommand>();
            _sessionAdapter = new UserSessionControllerAdapter(url, authData, updateInterval, timeout) { UseWebSockets = true };

            Initialize(url, updateInterval, timeout, true);
        }

        public UserCloudAgent(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
        {
            _authData = authData;
            _executedCommands = new List<DeviceCommand>();
            _sessionAdapter = new UserSessionControllerAdapter(url, uuid, authData, updateInterval, timeout) { UseWebSockets = true };

            Initialize(url, updateInterval, timeout, true);
        }

        public UserCloudAgent(SyncCloudAgent masterAgent, EltraDeviceNode device, uint updateInterval, uint timeout)
        {
            _authData = masterAgent.AuthData;
            _executedCommands = new List<DeviceCommand>();
            _sessionAdapter = new UserSessionControllerAdapter(masterAgent.Url, device.SessionUuid, masterAgent.AuthData, updateInterval, timeout) { UseWebSockets = true };

            Initialize(masterAgent.Url, updateInterval, timeout, false);
        }

        #endregion

        #region Events

        public event EventHandler<ParameterChangedEventArgs> ParameterChanged;

        public event EventHandler<SessionStatusChangedEventArgs> RemoteSessionStatusChanged;

        #endregion

        #region Events handling

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            ParameterChanged?.Invoke(sender, e);
        }

        private void OnSessionRegistered(object sender, SessionRegistrationEventArgs e)
        {
            _session = e.Session;

            if (e.Success)
            {
                _session.Status = SessionStatus.Online;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - OnSessionRegistered", $"Session uuid='{_session.Uuid}' registration failed!");
            }
        }
        private void OnRemoteSessionStatusChanged(object sender, SessionStatusChangedEventArgs e)
        {
            if(e.Status == SessionStatus.Online)
            {
                RecoverSession();
            }

            RemoteSessionStatusChanged?.Invoke(sender, e);
        }

        private void RecoverSession()
        {
            if (_deviceAuthData != null && _sessionAdapter != null)
            {
                Task.Run(async () =>
                {
                    await GetSessions(_sessionAdapter.Uuid, _deviceAuthData);
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

        #endregion

        #region Properties

        public string Uuid => _sessionAdapter.Uuid;

        public UserCloudAgentState State { get; set; } = UserCloudAgentState.Undefined;

        #endregion

        #region Methods

        private void Initialize(string url, uint updateInterval, uint timeout, bool useSessionUpdater)
        {
            _authentication = new Authentication(url);

            if (useSessionUpdater)
            {
                _sessionUpdater = new SessionUpdater(_sessionAdapter, updateInterval, timeout);
            }

            _executeCommander = new ExecuteCommander(_sessionAdapter);
            _parameterUpdateManager = new ParameterUpdateManager(_sessionAdapter);

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

            do
            {
                if (await RegisterSession())
                {
                    result = true;
                    break;
                }

                await Task.Delay(minWaitTime);

            } while (!token.IsCancellationRequested);

            return result;
        }

        private void StartSession()
        {
            _sessionUpdater?.Start();
            _executeCommander?.Start();
            _parameterUpdateManager?.Start();

            RegisterParameterUpdateManagerEvents();

            State = UserCloudAgentState.Started;
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
                StartSession();
                
                await SessionLoop(token);

                await StopSession();
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
            return await _sessionAdapter.Update();
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
                result = await _sessionAdapter.UnregisterSession();
            }

            return result;
        }

        public async Task<List<Session>> GetSessions(string uuid, UserAuthData authData)
        {
            _deviceAuthData = authData;

            var result = await _sessionAdapter.GetSessions(uuid, authData);

            return result;
        }

        private void RegisterEvents()
        {
            _sessionAdapter.SessionRegistered += OnSessionRegistered;

            _executeCommander.CommandExecuted += OnCommandExecuted;
            _executeCommander.RemoteSessionStatusChanged += OnRemoteSessionStatusChanged;
        }

        private async Task<List<EltraDeviceNode>> GetDeviceNodes(Session session, UserAuthData authData)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.GetDeviceNodes(session, authData);
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

        public async Task<List<EltraDeviceNodeList>> GetDevices(UserAuthData authData)
        {
            var result = new List<EltraDeviceNodeList>();

            if (await EnsureAgentReady())
            {
                var sessions = await GetSessions(_sessionAdapter.Uuid, authData);

                if (sessions != null)
                {
                    foreach (var session in sessions)
                    {
                        var deviceNodeList = new EltraDeviceNodeList() { Session = session };

                        var deviceNodes = await GetDeviceNodes(session, authData);

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

        public async Task<bool> CanLockDevice(EltraDevice device)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.CanLockDevice(device);
        }

        public async Task<bool> IsDeviceLocked(EltraDevice device)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.IsDeviceLocked(device);
        }

        public async Task<bool> LockDevice(EltraDevice device)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.LockDevice(device);
        }

        public async Task<bool> UnlockDevice(EltraDevice device)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.UnlockDevice(device);
        }
        
        public virtual async Task<DeviceCommand> GetDeviceCommand(EltraDeviceNode device, string commandName)
        {
            await EnsureAgentReady();

            var result = await _sessionAdapter.GetDeviceCommand(device, commandName);
            
            return result;
        }

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDeviceNode device)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.GetDeviceCommands(device);
        }

        public async Task<bool> PushCommand(DeviceCommand command, ExecCommandStatus status)
        {
            await EnsureAgentReady();

            command.Uuid = Guid.NewGuid().ToString();

            return await _sessionAdapter.PushCommand(command, Uuid, status);
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
                    if (deviceCommand.Uuid == command.Uuid)
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
                    MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand", $"command '{command.Uuid}' timeout, status = {command.Status}");
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

        public async Task<Parameter> GetParameter(ulong serialNumber, ushort index, byte subIndex)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.GetParameter(serialNumber, index, subIndex);
        }

        public async Task<ParameterValue> GetParameterValue(ulong serialNumber, ushort index, byte subIndex)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.GetParameterValue(serialNumber, index, subIndex);
        }

        public async Task<List<ParameterValue>> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.GetParameterHistory(serialNumber, uniqueId, from, to);
        }

        public async Task<List<ParameterUniqueIdValuePair>> GetParameterHistoryPair(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            await EnsureAgentReady();

            return await _sessionAdapter.GetParameterHistoryPair(serialNumber, uniqueId1, uniqueId2, from, to);
        }

        public async Task<bool> SignOut()
        {
            return await _authentication.SignOut();
        }

        public async Task<bool> SignUp(UserAuthData authData)
        {
            return await _authentication.SignUp(authData);
        }

        public async Task<bool> SignIn(UserAuthData authData)
        {
            return await _authentication.SignIn(authData);
        }

        #endregion
    }
}

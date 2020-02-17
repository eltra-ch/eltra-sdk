using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using EltraConnector.Controllers;
using EltraConnector.Events;
using EltraConnector.UserAgent.Events;
using EltraCommon.Logger;

using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.Contracts.Users;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraConnector.Ws;
using EltraCloudContracts.Contracts.Parameters;
using EltraConnector.Sessions;

namespace EltraConnector.UserAgent
{
    public class UserCloudAgent : IDisposable
    {
        #region Private fields

        private readonly Task _agentTask;
        private readonly CancellationTokenSource _agentCancelationTokenSource;
        private readonly SessionUpdater _sessionUpdater;
        private readonly ExecuteCommander _executeCommander;
        private readonly ParameterUpdateManager _parameterUpdateManager;        
        private readonly List<DeviceCommand> _executedCommands;
        private readonly WsConnectionManager _wsConnectionManager;

        private Session _session;

        #endregion

        #region Constructors

        public UserCloudAgent(string url, UserAuthData authData, uint updateInterval, uint timeout)
        {
            _wsConnectionManager = new WsConnectionManager() { HostUrl = url };
            
            _executedCommands = new List<DeviceCommand>();

            SessionAdapter = new UserSessionControllerAdapter(url, authData, updateInterval, timeout) { WsConnectionManager = _wsConnectionManager };

            _sessionUpdater = new SessionUpdater(SessionAdapter, updateInterval, timeout);
            _executeCommander = new ExecuteCommander(SessionAdapter);
            _parameterUpdateManager = new ParameterUpdateManager(SessionAdapter);

            RegisterEvents();

            _agentCancelationTokenSource = new CancellationTokenSource();

            _agentTask = Task.Run(() => StartSession(_agentCancelationTokenSource.Token));
        }

        public UserCloudAgent(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
        {
            _wsConnectionManager = new WsConnectionManager() { HostUrl = url };
            
            _executedCommands = new List<DeviceCommand>();

            SessionAdapter = new UserSessionControllerAdapter(url, uuid, authData, updateInterval, timeout) { WsConnectionManager = _wsConnectionManager };

            _sessionUpdater = new SessionUpdater(SessionAdapter, updateInterval, timeout);
            _executeCommander = new ExecuteCommander(SessionAdapter);
            _parameterUpdateManager = new ParameterUpdateManager(SessionAdapter);

            RegisterEvents();

            _agentCancelationTokenSource = new CancellationTokenSource();

            _agentTask = Task.Run(() => StartSession(_agentCancelationTokenSource.Token));
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
            RemoteSessionStatusChanged?.Invoke(sender, e);
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

        private UserSessionControllerAdapter SessionAdapter { get; }

        public string Uuid => SessionAdapter.Uuid;

        #endregion

        #region Methods

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
        
        private async Task StartSession(CancellationToken token)
        {
            const int minWaitTime = 100;

            do
            {
                if(await RegisterSession())
                {
                    break;
                }
                
                await Task.Delay(minWaitTime);

            } while (!token.IsCancellationRequested);

            _sessionUpdater.Start();
            _executeCommander.Start();
            _parameterUpdateManager.Start();
                
            RegisterParameterUpdateManagerEvents();

            do
            {
                
                await Task.Delay(minWaitTime);

            } while (!token.IsCancellationRequested);

            _executeCommander.Stop();
            _parameterUpdateManager.Stop();
            _sessionUpdater.Stop();
                
            await UnregisterSession();

            UnregisterParameterUpdateManagerEvents();

            MsgLogger.WriteLine($"Sync agent working thread finished successfully!");
            
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
            return await SessionAdapter.Update();
        }

        private async Task<bool> RegisterSession()
        {
            var result = await SessionAdapter.Update();

            return result;
        }

        private async Task<bool> UnregisterSession()
        {
            var result = await SessionAdapter.UnregisterSession();

            return result;
        }

        public async Task<List<Session>> GetSessions(string uuid, UserAuthData authData)
        {
            var result = new List<Session>();

            if (!await SessionAdapter.IsSessionRegistered(uuid))
            {
                if(await SessionAdapter.RegisterSession())
                {
                    result = await SessionAdapter.GetSessions(uuid, authData);
                }
            }
            else
            {
                result = await SessionAdapter.GetSessions(uuid, authData);
            }
            
            return result;
        }

        private void RegisterEvents()
        {
            SessionAdapter.SessionRegistered += OnSessionRegistered;

            _executeCommander.CommandExecuted += OnCommandExecuted;
            _executeCommander.RemoteSessionStatusChanged += OnRemoteSessionStatusChanged;
        }

        public async Task<List<EltraDevice>> GetSessionDevices(Session session, UserAuthData authData)
        {
            return await SessionAdapter.GetSessionDevices(session, authData);
        }

        public async Task<List<(Session, EltraDevice)>> GetDevices(UserAuthData authData)
        {
            var result = new List<(Session,EltraDevice)>();
            var sessions = await GetSessions(SessionAdapter.Uuid, authData);

            if (sessions != null)
            {
                foreach (var session in sessions)
                {
                    var devices = await GetSessionDevices(session, authData);

                    if (devices != null)
                    {
                        foreach (var device in devices)
                        {
                            result.Add((session, device));
                        }
                    }
                }
            }

            return result;
        }

        public async Task<bool> CanLockDevice(EltraDevice device)
        {
            return await SessionAdapter.CanLockDevice(device);
        }

        public async Task<bool> IsDeviceLocked(EltraDevice device)
        {
            return await SessionAdapter.IsDeviceLocked(device);
        }

        public async Task<bool> LockDevice(EltraDevice device)
        {
            return await SessionAdapter.LockDevice(device);
        }

        public async Task<bool> UnlockDevice(EltraDevice device)
        {
            return await SessionAdapter.UnlockDevice(device);
        }
        
        public virtual async Task<DeviceCommand> GetDeviceCommand(EltraDevice device, string commandName)
        {
            var result = await SessionAdapter.GetDeviceCommand(device, commandName);
            
            return result;
        }

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDevice device)
        {
            return await SessionAdapter.GetDeviceCommands(device);
        }

        public async Task<bool> PushCommand(DeviceCommand command, ExecCommandStatus status)
        {
            command.Uuid = Guid.NewGuid().ToString();

            return await SessionAdapter.PushCommand(command, Uuid, status);
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
            
            if (await PushCommand(command, ExecCommandStatus.Waiting))
            {
                _executeCommander.FollowCommand(command);

                result = true;
            }

            return result;
        }

        public async Task<Parameter> GetParameter(ulong serialNumber, ushort index, byte subIndex)
        {
            return await SessionAdapter.GetParameter(serialNumber, index, subIndex);
        }

        public async Task<ParameterValue> GetParameterValue(ulong serialNumber, ushort index, byte subIndex)
        {
            return await SessionAdapter.GetParameterValue(serialNumber, index, subIndex);
        }

        public async Task<List<ParameterValue>> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to)
        {
            return await SessionAdapter.GetParameterHistory(serialNumber, uniqueId, from, to);
        }

        public async Task<List<ParameterUniqueIdValuePair>> GetParameterHistoryPair(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            return await SessionAdapter.GetParameterHistoryPair(serialNumber, uniqueId1, uniqueId2, from, to);
        }

        #endregion


    }
}

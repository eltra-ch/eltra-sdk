using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraConnector.Controllers;
using EltraConnector.UserAgent.Events;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using EltraConnector.Ws;
using EltraCloudContracts.Contracts.Sessions;
using EltraCommon.Threads;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EltraConnector.Events;

namespace EltraConnector.UserAgent
{
    class ExecuteCommander : EltraThread
    {
        #region Private fields

        private readonly UserSessionControllerAdapter _sessionAdapter;
        private readonly List<DeviceCommand> _deviceCommands;
        private readonly WsConnectionManager _wsConnectionManager;
        private string _commandExecUuid;
        private string _wsChannelName;

        #endregion

        #region Constructors

        public ExecuteCommander(UserSessionControllerAdapter sessionAdapter)
        {
            _deviceCommands = new List<DeviceCommand>();

            _wsConnectionManager = new WsConnectionManager() { HostUrl = sessionAdapter.Url };

            _sessionAdapter = sessionAdapter;
            _commandExecUuid = _sessionAdapter.Uuid + "_ExecCommander";
            _wsChannelName = "ExecuteCommander";
        }

        #endregion
        
        #region Events

        public event EventHandler<ExecuteCommanderEventArgs> CommandExecuted;

        public event EventHandler<SessionStatusChangedEventArgs> RemoteSessionStatusChanged;

        protected virtual void OnCommandExecuted(ExecuteCommanderEventArgs e)
        {
            CommandExecuted?.Invoke(this, e);
        }

        protected virtual void OnRemoteSessionStatusChanged(SessionStatusChangedEventArgs args)
        {
            RemoteSessionStatusChanged?.Invoke(this, args);
        }

        #endregion

        #region Properties

        public bool UseRest { get; set; }

        #endregion

        #region Methods

        public override bool Stop()
        {
            if(_wsConnectionManager.IsConnected(_commandExecUuid))
            {
                Task.Run(async ()=>{ await _wsConnectionManager.Disconnect(_commandExecUuid); }).GetAwaiter().GetResult();
            }
            
            return base.Stop();
        }

        private async Task SendSessionIdentyfication(string commandExecUuid)
        {
            if (_wsConnectionManager.IsConnected(commandExecUuid))
            {
                var sessionIdent = new SessionIdentification() { Uuid = _sessionAdapter.Uuid };

                await _wsConnectionManager.Send(commandExecUuid, sessionIdent);
            }
        }

        private void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var msg = errorArgs.ErrorContext.Error.Message;

            MsgLogger.WriteDebug($"{GetType().Name} - HandleDeserializationError", msg);

            errorArgs.ErrorContext.Handled = true;
        }

        protected override async Task Execute()
        {
            const int executeIntervalRest = 100;
            const int executeIntervalWs = 10;

            MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"create exec channel '{_wsChannelName}', uuid='{_commandExecUuid}'");

            await CreateWsChannel(_commandExecUuid, _wsChannelName);

            while (ShouldRun())
            {
                try
                {
                    var processingCommands = GetProcessingCommands();

                    if (_wsConnectionManager.IsConnected(_commandExecUuid))
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{_wsChannelName}', uuid='{_commandExecUuid}' - receive...");

                        var json = await _wsConnectionManager.Receive(_commandExecUuid);

                        try
                        {
                            if (WsConnection.IsJson(json))
                            {
                                var executeCommands = JsonConvert.DeserializeObject<List<ExecuteCommand>>(json, new JsonSerializerSettings
                                {
                                    Error = HandleDeserializationError
                                });

                                if (executeCommands != null)
                                {
                                    MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{_wsChannelName}', uuid='{_commandExecUuid}' - received {executeCommands.Count} commands");

                                    foreach (var executeCommand in executeCommands)
                                    {
                                        await WsPopCommand(executeCommand);
                                    }
                                }
                                else
                                {
                                    var executeCommandStatusList = JsonConvert.DeserializeObject<List<ExecuteCommandStatus>>(json, new JsonSerializerSettings
                                    {
                                        Error = HandleDeserializationError
                                    });

                                    if (executeCommandStatusList != null)
                                    {
                                        MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{_wsChannelName}', uuid='{_commandExecUuid}' - received {executeCommandStatusList.Count} command status");

                                        foreach (var executeCommandStatus in executeCommandStatusList)
                                        {
                                            await WsPopCommand(executeCommandStatus);
                                        }
                                    }
                                    else
                                    {
                                        var sessionStatusUpdate = JsonConvert.DeserializeObject<SessionStatusUpdate>(json, new JsonSerializerSettings
                                        {
                                            Error = HandleDeserializationError
                                        });

                                        if (sessionStatusUpdate != null)
                                        {
                                            if (sessionStatusUpdate.SessionUuid != _sessionAdapter.Uuid)
                                            {
                                                MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"session {sessionStatusUpdate.SessionUuid}, status changed to {sessionStatusUpdate.Status}");

                                                OnRemoteSessionStatusChanged(new SessionStatusChangedEventArgs() { Uuid = sessionStatusUpdate.SessionUuid, Status = sessionStatusUpdate.Status } );
                                            }
                                        }
                                        else
                                        {
                                            MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{_wsChannelName}', uuid='{_commandExecUuid}' - received 0 commands");
                                        }
                                    }

                                }
                            }
                            else if (string.IsNullOrEmpty(json) && _wsConnectionManager.IsConnected(_commandExecUuid))
                            {
                                MsgLogger.WriteLine($"empty string received");
                            }
                        }
                        catch(Exception e)
                        {
                            MsgLogger.Exception($"{GetType().Name} - Execute", e);
                        }

                        if (ShouldRun())
                        { 
                            await Task.Delay(executeIntervalWs);
                        }
                    }
                    else if(UseRest)
                    {
                        foreach (var deviceCommand in processingCommands)
                        {
                            await PopCommand(deviceCommand);
                        }

                        if (ShouldRun())
                        { 
                            await Task.Delay(executeIntervalRest);
                        }
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Execute", e);
                }

                if (ShouldRun())
                {
                    if (!_wsConnectionManager.IsConnected(_commandExecUuid))
                    {
                        await CreateWsChannel(_commandExecUuid, _wsChannelName);
                    }
                }
            }

            MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"exec channel '{_wsChannelName}', uuid='{_commandExecUuid}' closed");
        }

        private async Task CreateWsChannel(string commandExecUuid, string wsChannelName)
        {
            if (_wsConnectionManager.CanConnect(commandExecUuid))
            {
                if (await _wsConnectionManager.Connect(commandExecUuid, wsChannelName))
                {
                    await SendSessionIdentyfication(commandExecUuid);
                }
            }
        }

        private List<DeviceCommand> GetProcessingCommands()
        {
            var result = new List<DeviceCommand>(); 

            lock (this)
            {
                foreach (var deviceCommand in _deviceCommands)
                {
                    var clonedCommand = deviceCommand.Clone();
                    
                    result.Add(clonedCommand);
                }
            }

            return result;
        }

        private async Task<ExecuteCommand> PopCommand(string commandUuid, EltraDevice device, ExecCommandStatus status)
        {
            return await _sessionAdapter.PopCommand(commandUuid, device, status);
        }

        private async Task<bool> SetCommandStatus(ExecuteCommand executeCommand, ExecCommandStatus status)
        {
            return await _sessionAdapter.SetCommandStatus(executeCommand, status);
        }

        private async Task<bool> SetCommandStatus(ExecuteCommandStatus executeCommandStatus)
        {
            return await _sessionAdapter.SetCommandStatus(executeCommandStatus);
        }

        private async Task<ExecuteCommandStatus> GetCommandStatus(ExecuteCommand command)
        {
            return await _sessionAdapter.GetCommandStatus(command);
        }

        public async Task WsPopCommand(ExecuteCommand execCommand)
        {
            var deviceCommand = execCommand?.Command;

            if(deviceCommand!=null)
            {
                var followedCommand = FindDeviceCommand(deviceCommand.Uuid);

                switch (deviceCommand.Status)
                {
                    case ExecCommandStatus.Executed:
                    {
                        await SetCommandStatus(execCommand, ExecCommandStatus.Complete);

                        OnCommandExecuted(new ExecuteCommanderEventArgs
                        {
                            CommandUuid = deviceCommand.Uuid,
                            Status = ExecCommandStatus.Executed,
                            Command = deviceCommand
                        });

                        RemoveDeviceCommand(deviceCommand.Uuid);

                    } break;
                    case ExecCommandStatus.Failed:
                    case ExecCommandStatus.Refused:
                        {
                        MsgLogger.WriteError($"{GetType().Name} - WsPopCommand",
                            $"Command {deviceCommand.Name} (uuid='{deviceCommand.Uuid}') execution failed!, status={deviceCommand.Status}");

                            OnCommandExecuted(new ExecuteCommanderEventArgs
                            {
                                CommandUuid = deviceCommand.Uuid,
                                Status = ExecCommandStatus.Failed,
                                Command = deviceCommand
                            });

                            RemoveDeviceCommand(deviceCommand.Uuid);
                    } break;
                    
                }

                if (followedCommand != null)
                {
                    followedCommand.Status = deviceCommand.Status;
                }
            }
        }

        public async Task WsPopCommand(ExecuteCommandStatus execCommandStatus)
        {  
            if (execCommandStatus != null)
            {
                switch (execCommandStatus.Status)
                {
                    case ExecCommandStatus.Executed:
                        {
                            execCommandStatus.Status = ExecCommandStatus.Complete;

                            if (await SetCommandStatus(execCommandStatus))
                            {
                                RemoveDeviceCommand(execCommandStatus.CommandUuid);

                                OnCommandExecuted(new ExecuteCommanderEventArgs { CommandUuid = execCommandStatus.CommandUuid, Status = execCommandStatus.Status });
                            }
                        }
                        break;
                    case ExecCommandStatus.Failed:
                        {
                            RemoveDeviceCommand(execCommandStatus.CommandUuid);

                            MsgLogger.WriteError($"{GetType().Name} - WsPopCommand",
                                $"Command {execCommandStatus.CommandName} (uuid='{execCommandStatus.CommandUuid}') execution failed!");
                        }
                        break;
                    case ExecCommandStatus.Refused:
                        {
                            RemoveDeviceCommand(execCommandStatus.CommandUuid);

                            MsgLogger.WriteError($"{GetType().Name} - WsPopCommand",
                                $"Command {execCommandStatus.CommandName} (uuid='{execCommandStatus.CommandUuid}') execution refused!");
                        }
                        break;
                }
            }
        }

        public async Task<DeviceCommand> PopCommand(DeviceCommand command)
        {
            const double timeout = 30; 
            DeviceCommand result = null;

            var commandStatus = await GetCommandStatus(new ExecuteCommand {Command = command, SessionUuid = _sessionAdapter.Uuid});

            if (commandStatus != null)
            {
                switch (commandStatus.Status)
                {
                    case ExecCommandStatus.Waiting:
                    {
                        if((DateTime.Now - commandStatus.Modified).TotalSeconds > timeout)
                        {
                            RemoveDeviceCommand(command.Uuid);
                        }
                    } break;
                    case ExecCommandStatus.Complete:
                    {
                        RemoveDeviceCommand(command.Uuid);
                    } break;
                    case ExecCommandStatus.Executed:
                    {
                        var execCommand = await PopCommand(command.Uuid, command.Device, ExecCommandStatus.Executed);

                        if (execCommand != null)
                        {
                            if (await SetCommandStatus(execCommand, ExecCommandStatus.Complete))
                            {
                                RemoveDeviceCommand(command.Uuid);

                                result = execCommand.Command;

                                OnCommandExecuted(new ExecuteCommanderEventArgs { CommandUuid = command.Uuid, Command = command, Status = ExecCommandStatus.Complete });
                            }
                        }
                    } break;
                    case ExecCommandStatus.Failed:
                    {
                        RemoveDeviceCommand(command.Uuid);

                        MsgLogger.WriteError($"{GetType().Name} - PopCommand",
                            $"Command {command.Name} (uuid='{command.Uuid}') execution failed!");
                    } break;
                    case ExecCommandStatus.Refused:
                    {
                        RemoveDeviceCommand(command.Uuid);

                        MsgLogger.WriteError($"{GetType().Name} - PopCommand",
                            $"Command {command.Name} (uuid='{command.Uuid}') execution refused!");
                    } break;
                }
            }

            return result;
        }

        private DeviceCommand FindDeviceCommand(string uuid)
        {
            DeviceCommand result = null;

            lock (this)
            {
                foreach (var command in _deviceCommands)
                {
                    if (command.Uuid == uuid)
                    {
                        result = command;
                        break;
                    }
                }
            }

            return result;
        }

        private void RemoveDeviceCommand(string uuid)
        {
            var deviceCommand = FindDeviceCommand(uuid);

            lock (this)
            {
                if (_deviceCommands.Contains(deviceCommand))
                {
                    _deviceCommands.Remove(deviceCommand);
                }
            }
        }

        public void FollowCommand(DeviceCommand command)
        {
            lock (this)
            {
                _deviceCommands.Add(command);
            }
        }

        #endregion
    }
}

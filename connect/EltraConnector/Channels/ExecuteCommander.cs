using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraConnector.Controllers;
using EltraConnector.UserAgent.Events;
using EltraCommon.Logger;
using EltraCommon.Contracts.CommandSets;
using EltraConnector.Transport.Ws;
using EltraCommon.Contracts.Channels;
using EltraCommon.Threads;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EltraConnector.Events;
using EltraCommon.Contracts.Devices;

namespace EltraConnector.Channels
{
    class ExecuteCommander : WsChannelThread
    {
        #region Private fields

        const string ChannelName = "ExecuteCommander";

        private readonly UserChannelControllerAdapter _channelAdapter;
        private readonly List<DeviceCommand> _deviceCommands;        
        private int _nodeId;

        #endregion

        #region Constructors

        public ExecuteCommander(UserChannelControllerAdapter channelAdapter)
            : base(channelAdapter.WsConnectionManager, channelAdapter.ChannelId + "_ExecCommander", ChannelName,
                  channelAdapter.ChannelId, 0, channelAdapter.User.Identity)
        {
            _deviceCommands = new List<DeviceCommand>();           

            _channelAdapter = channelAdapter;
        }

        public ExecuteCommander(UserChannelControllerAdapter channelAdapter, int nodeId)
            : base(channelAdapter.WsConnectionManager, channelAdapter.ChannelId + $"_ExecCommander_{nodeId}", ChannelName,
                  channelAdapter.ChannelId, nodeId, channelAdapter.User.Identity)
        {
            _nodeId = nodeId;

            _deviceCommands = new List<DeviceCommand>();

            _channelAdapter = channelAdapter;
        }

        #endregion

        #region Events

        public event EventHandler<ExecuteCommanderEventArgs> CommandExecuted;

        public event EventHandler<AgentChannelStatusChangedEventArgs> RemoteChannelStatusChanged;

        #endregion

        #region Events handling

        protected virtual void OnCommandExecuted(ExecuteCommanderEventArgs e)
        {
            CommandExecuted?.Invoke(this, e);
        }

        protected virtual void OnRemoteChannelStatusChanged(AgentChannelStatusChangedEventArgs args)
        {
            RemoteChannelStatusChanged?.Invoke(this, args);
        }

        #endregion

        #region Properties

        public bool UseRest { get; set; }

        #endregion

        #region Methods

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

            MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"create exec channel '{WsChannelName}', uuid='{WsChannelId}'");

            await ConnectToWsChannel();

            while (ShouldRun())
            {
                try
                {
                    var processingCommands = GetProcessingCommands();

                    if (WsConnectionManager.IsConnected(WsChannelId))
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{WsChannelName}', uuid='{WsChannelId}' - receive...");

                        var json = await WsConnectionManager.Receive(WsChannelId);

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
                                    MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{WsChannelName}', uuid='{WsChannelId}' - received {executeCommands.Count} commands");

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
                                        MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{WsChannelName}', uuid='{WsChannelId}' - received {executeCommandStatusList.Count} command status");

                                        foreach (var executeCommandStatus in executeCommandStatusList)
                                        {
                                            await WsPopCommand(executeCommandStatus);
                                        }
                                    }
                                    else
                                    {
                                        var channelStatusUpdate = JsonConvert.DeserializeObject<ChannelStatusUpdate>(json, new JsonSerializerSettings
                                        {
                                            Error = HandleDeserializationError
                                        });

                                        if (channelStatusUpdate != null)
                                        {
                                            if (channelStatusUpdate.ChannelId != _channelAdapter.ChannelId)
                                            {
                                                MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"session {channelStatusUpdate.ChannelId}, status changed to {channelStatusUpdate.Status}");

                                                OnRemoteChannelStatusChanged(new AgentChannelStatusChangedEventArgs() { Id = channelStatusUpdate.ChannelId, Status = channelStatusUpdate.Status });
                                            }
                                        }
                                        else
                                        {
                                            MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{WsChannelName}', uuid='{WsChannelId}' - received 0 commands");
                                        }
                                    }

                                }
                            }
                            else if (string.IsNullOrEmpty(json) && WsConnectionManager.IsConnected(WsChannelId))
                            {
                                MsgLogger.WriteLine($"empty string received");
                            }
                        }
                        catch (Exception e)
                        {
                            MsgLogger.Exception($"{GetType().Name} - Execute", e);
                        }

                        if (ShouldRun())
                        {
                            await Task.Delay(executeIntervalWs);
                        }
                    }
                    else if (UseRest)
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
                    await ReconnectToWsChannel();
                }
            }

            await DisconnectFromWsChannel();

            MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"exec channel '{WsChannelName}', uuid='{WsChannelId}' closed");
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
            return await _channelAdapter.PopCommand(commandUuid, device, status);
        }

        private async Task<bool> SetCommandStatus(ExecuteCommand executeCommand, ExecCommandStatus status)
        {
            return await _channelAdapter.SetCommandStatus(executeCommand, status);
        }

        private async Task<bool> SetCommandStatus(ExecuteCommandStatus executeCommandStatus)
        {
            return await _channelAdapter.SetCommandStatus(executeCommandStatus);
        }

        private async Task<ExecuteCommandStatus> GetCommandStatus(ExecuteCommand command)
        {
            return await _channelAdapter.GetCommandStatus(command);
        }

        public async Task WsPopCommand(ExecuteCommand execCommand)
        {
            var deviceCommand = execCommand?.Command;

            if (deviceCommand != null)
            {
                var followedCommand = FindDeviceCommand(deviceCommand.Id);

                switch (deviceCommand.Status)
                {
                    case ExecCommandStatus.Executed:
                        {
                            await SetCommandStatus(execCommand, ExecCommandStatus.Complete);

                            OnCommandExecuted(new ExecuteCommanderEventArgs
                            {
                                CommandUuid = deviceCommand.Id,
                                Status = ExecCommandStatus.Executed,
                                Command = deviceCommand
                            });

                            RemoveDeviceCommand(deviceCommand.Id);

                        }
                        break;
                    case ExecCommandStatus.Failed:
                    case ExecCommandStatus.Refused:
                        {
                            MsgLogger.WriteError($"{GetType().Name} - WsPopCommand",
                                $"Command {deviceCommand.Name} (uuid='{deviceCommand.Id}') execution failed!, status={deviceCommand.Status}");

                            OnCommandExecuted(new ExecuteCommanderEventArgs
                            {
                                CommandUuid = deviceCommand.Id,
                                Status = ExecCommandStatus.Failed,
                                Command = deviceCommand
                            });

                            RemoveDeviceCommand(deviceCommand.Id);
                        }
                        break;

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
                                RemoveDeviceCommand(execCommandStatus.CommandId);

                                OnCommandExecuted(new ExecuteCommanderEventArgs { CommandUuid = execCommandStatus.CommandId, Status = execCommandStatus.Status });
                            }
                        }
                        break;
                    case ExecCommandStatus.Failed:
                        {
                            RemoveDeviceCommand(execCommandStatus.CommandId);

                            MsgLogger.WriteError($"{GetType().Name} - WsPopCommand",
                                $"Command {execCommandStatus.CommandName} (uuid='{execCommandStatus.CommandId}') execution failed!");
                        }
                        break;
                    case ExecCommandStatus.Refused:
                        {
                            RemoveDeviceCommand(execCommandStatus.CommandId);

                            MsgLogger.WriteError($"{GetType().Name} - WsPopCommand",
                                $"Command {execCommandStatus.CommandName} (uuid='{execCommandStatus.CommandId}') execution refused!");
                        }
                        break;
                }
            }
        }

        public async Task<DeviceCommand> PopCommand(DeviceCommand command)
        {
            const double timeout = 30;
            DeviceCommand result = null;

            var commandStatus = await GetCommandStatus(new ExecuteCommand
            {
                Command = command,
                SourceChannelId = _channelAdapter.ChannelId,
                TargetChannelId = command.Device.ChannelId,
                NodeId = command.Device.NodeId
            });

            if (commandStatus != null)
            {
                switch (commandStatus.Status)
                {
                    case ExecCommandStatus.Waiting:
                        {
                            if ((DateTime.Now - commandStatus.Modified).TotalSeconds > timeout)
                            {
                                RemoveDeviceCommand(command.Id);
                            }
                        }
                        break;
                    case ExecCommandStatus.Complete:
                        {
                            RemoveDeviceCommand(command.Id);
                        }
                        break;
                    case ExecCommandStatus.Executed:
                        {
                            var execCommand = await PopCommand(command.Id, command.Device, ExecCommandStatus.Executed);

                            if (execCommand != null)
                            {
                                if (await SetCommandStatus(execCommand, ExecCommandStatus.Complete))
                                {
                                    RemoveDeviceCommand(command.Id);

                                    result = execCommand.Command;

                                    OnCommandExecuted(new ExecuteCommanderEventArgs { CommandUuid = command.Id, Command = command, Status = ExecCommandStatus.Complete });
                                }
                            }
                        }
                        break;
                    case ExecCommandStatus.Failed:
                        {
                            RemoveDeviceCommand(command.Id);

                            MsgLogger.WriteError($"{GetType().Name} - PopCommand",
                                $"Command {command.Name} (uuid='{command.Id}') execution failed!");
                        }
                        break;
                    case ExecCommandStatus.Refused:
                        {
                            RemoveDeviceCommand(command.Id);

                            MsgLogger.WriteError($"{GetType().Name} - PopCommand",
                                $"Command {command.Name} (uuid='{command.Id}') execution refused!");
                        }
                        break;
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
                    if (command.Id == uuid)
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

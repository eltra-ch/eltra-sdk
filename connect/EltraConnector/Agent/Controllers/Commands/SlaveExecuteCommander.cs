using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraConnector.UserAgent.Events;
using EltraCommon.Logger;
using EltraCommon.Contracts.CommandSets;
using EltraConnector.Transport.Ws;
using EltraCommon.Contracts.Channels;
//using Newtonsoft.Json.Serialization;
using EltraConnector.Events;
using EltraCommon.Contracts.Devices;
using EltraConnector.Transport.Events;
using EltraConnector.Channels;
using EltraConnector.Transport.Udp;
using EltraConnector.Transport.Udp.Contracts;
using EltraConnector.Extensions;
using EltraConnector.Controllers.Commands;

namespace EltraConnector.Agent.Controllers.Commands
{
    class SlaveExecuteCommander : WsChannelThread
    {
        #region Private fields

        const string ChannelName = "ExecuteCommander";

        private readonly SlaveChannelControllerAdapter _channelAdapter;
        private readonly List<DeviceCommand> _deviceCommands;        
        private int _nodeId;
        private readonly ExecuteCommandCache _executeCommandCache;

        #endregion

        #region Constructors

        public SlaveExecuteCommander(SlaveChannelControllerAdapter channelAdapter)
            : base(channelAdapter.ConnectionManager, channelAdapter.ChannelId + "_ExecCommander", ChannelName,
                  channelAdapter.ChannelId, 0, channelAdapter.User.Identity)
        {
            _deviceCommands = new List<DeviceCommand>();
            _executeCommandCache = new ExecuteCommandCache();

            _channelAdapter = channelAdapter;
        }

        public SlaveExecuteCommander(SlaveChannelControllerAdapter channelAdapter, int nodeId)
            : base(channelAdapter.ConnectionManager, channelAdapter.ChannelId + $"_ExecCommander_{nodeId}", ChannelName,
                  channelAdapter.ChannelId, nodeId, channelAdapter.User.Identity)
        {
            _nodeId = nodeId;

            _deviceCommands = new List<DeviceCommand>();
            _executeCommandCache = new ExecuteCommandCache();

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

        private void OnMessageReceived(object sender, ConnectionMessageEventArgs e)
        {
            if (sender is WsConnection connection && connection.UniqueId == WsChannelId)
            {
                Task.Run(async () =>
                {
                    if (e.Type == MessageType.Data)
                    {
                        await HandleMsgReceived(e.Message);
                    }
                });
            }
            else if(sender is UdpClientConnection udpConnection && udpConnection.UniqueId == WsChannelId)
            {
                var udpRequest = System.Text.Json.JsonSerializer.Deserialize<UdpRequest>(e.Message);

                if (udpRequest is UdpRequest)
                {
                    if (e.Type == MessageType.Text)
                    {
                        Task.Run(async () =>
                        {
                            await HandleMsgReceived(udpRequest.Data);
                        });
                    }
                }
            }
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        protected override async Task Execute()
        {
            const int executeIntervalRest = 100;
            const int executeIntervalWs = 1;
            const int reconnectIntervalWs = 1000;

            MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"create exec channel '{WsChannelName}', uuid='{WsChannelId}'");

            await ConnectToChannel();

            ConnectionManager.MessageReceived += OnMessageReceived;

            while (ShouldRun())
            {
                try
                {
                    var processingCommands = GetProcessingCommands();

                    if (ConnectionManager.IsConnected(WsChannelId))
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{WsChannelName}', uuid='{WsChannelId}' - receive...");

                        await ConnectionManager.Receive(WsChannelId, ShouldRun);
                    }
                    else
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

                    if (ConnectionManager.IsConnected(WsChannelId))
                    {
                        await Task.Delay(executeIntervalWs);
                    }
                    else
                    {
                        await Task.Delay(reconnectIntervalWs);
                    }
                }
            }
            ConnectionManager.MessageReceived -= OnMessageReceived;

            await DisconnectFromWsChannel();

            MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"exec channel '{WsChannelName}', uuid='{WsChannelId}' closed");
        }

        private async Task HandleMsgReceived(string json)
        {
            try
            {
                if (WsConnection.IsJson(json))
                {
                    await ProcessJsonMessage(json);
                }
                else if (string.IsNullOrEmpty(json) && ConnectionManager.IsConnected(WsChannelId))
                {
                    MsgLogger.WriteLine($"empty string received");
                }
                else if (!string.IsNullOrEmpty(json))
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"unknown message '{json}' received");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Execute", e);
            }
        }

        private ExecuteCommand ParseExecuteCommand(string json)
        {
            ExecuteCommand result = null;

            try
            {
                var executeCommand = System.Text.Json.JsonSerializer.Deserialize<ExecuteCommand>(json);

                if (executeCommand != null && executeCommand.IsValid())
                {
                    result = executeCommand;
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        private ExecuteCommandStatus ParseExecuteCommandStatus(string json)
        {
            ExecuteCommandStatus result = null;

            try
            {
                var executeCommandStatus = System.Text.Json.JsonSerializer.Deserialize<ExecuteCommandStatus>(json);

                if (executeCommandStatus != null && executeCommandStatus.IsValid())
                {
                    result = executeCommandStatus;
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        private List<ExecuteCommandStatus> ParseExecuteCommandStatusSet(string json)
        {
            List<ExecuteCommandStatus> result = null;

            try
            {
                var executeCommandStatusList = System.Text.Json.JsonSerializer.Deserialize<List<ExecuteCommandStatus>>(json);

                if (executeCommandStatusList != null && executeCommandStatusList.Count > 0)
                {
                    result = new List<ExecuteCommandStatus>();

                    foreach (var executeCommandStatus in executeCommandStatusList)
                    {
                        if (executeCommandStatus.IsValid())
                        {
                            result.Add(executeCommandStatus);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        private List<ExecuteCommand> ParseExecuteCommandSet(string json)
        {
            List<ExecuteCommand> result = null;

            try
            {
                var executeCommands = System.Text.Json.JsonSerializer.Deserialize<List<ExecuteCommand>>(json);

                if (executeCommands != null && executeCommands.Count > 0)
                {
                    result = new List<ExecuteCommand>();

                    foreach (var executeCommand in executeCommands)
                    {
                        if (executeCommand.IsValid())
                        {
                            result.Add(executeCommand);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        private ChannelStatusUpdate ParseChannelStatusUpdate(string json)
        {
            ChannelStatusUpdate result = null;

            try
            {
                var channelStatusUpdate = System.Text.Json.JsonSerializer.Deserialize<ChannelStatusUpdate>(json);

                if (channelStatusUpdate != null && channelStatusUpdate.IsValid())
                {
                    result = channelStatusUpdate;
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        private async Task ProcessJsonMessage(string json)
        {
            var executeCommands = ParseExecuteCommandSet(json);

            if (executeCommands != null)
            {
                MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"channel '{WsChannelName}', uuid='{WsChannelId}' - received {executeCommands.Count} commands");

                foreach (var executeCommand in executeCommands)
                {
                    await ExecuteCommand(executeCommand);
                }
            }
            else
            {
                var executeCommand = ParseExecuteCommand(json);

                if (executeCommand != null)
                {
                    await ExecuteCommand(executeCommand);
                }
                else
                {
                    var executeCommandStatusSet = ParseExecuteCommandStatusSet(json);

                    if (executeCommandStatusSet != null)
                    {
                        foreach (var executeCommandStatus in executeCommandStatusSet)
                        {
                            await WsPopCommand(executeCommandStatus);
                        }
                    }
                    else
                    {
                        var executeCommandStatus = ParseExecuteCommandStatus(json);

                        if (executeCommandStatus != null)
                        {
                            await WsPopCommand(executeCommandStatus);

                            MsgLogger.WriteFlow($"{GetType().Name} - ProcessJsonCommand", $"command {executeCommandStatus.CommandName} status changed = {executeCommandStatus.Status}");
                        }
                        else
                        {
                            var channelStatusUpdate = ParseChannelStatusUpdate(json);

                            if (channelStatusUpdate != null)
                            {
                                if (channelStatusUpdate.ChannelId != _channelAdapter.Channel.Id)
                                {
                                    MsgLogger.WriteDebug($"{GetType().Name} - ProcessJsonCommand", $"session {channelStatusUpdate.ChannelId}, status changed to {channelStatusUpdate.Status}");

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

        public async Task ExecuteCommand(ExecuteCommand execCommand)
        {
            var deviceCommand = execCommand?.Command;

            if (deviceCommand != null && _executeCommandCache.CanExecute(execCommand))
            {
                var followedCommand = FindDeviceCommand(deviceCommand.Id);

                if (followedCommand != null)
                {
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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using EltraCloud.Services;
using EltraCloud.Services.Events;
using EltraCloud.Channels.Interfaces;
using EltraCloud.Channels.Readers;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.Contracts.Ws;
using EltraCommon.Logger;
using Newtonsoft.Json;

#pragma warning disable CS1591

namespace EltraCloud.Channels.Processors
{
    public class SlaveCommandChannelProcessor : IChannelProcessor
    {
        #region Private fields

        private readonly object _lock = new object();

        private SessionIdentification _sessionIdentification;
        private string _channelName;
        
        private readonly AsyncChannelReader _asyncChannelReader;
        private readonly Stack<ExecuteCommand> _commandStack;
        private readonly Stack<ExecuteCommandStatus> _commandChangedStack;

        #endregion

        #region Constructors

        public SlaveCommandChannelProcessor(IPAddress source, WebSocket webSocket, ISessionService sessionService)
            : base(source, webSocket, sessionService)
        {
            _commandStack = new Stack<ExecuteCommand>();
            _commandChangedStack = new Stack<ExecuteCommandStatus>();

            _asyncChannelReader = new AsyncChannelReader(source, webSocket);

            Reader = _asyncChannelReader;
        }

        #endregion

        #region Events handling

        private async void OnSessionStatusChanged(object sender, SessionStatusChangedEventArgs e)
        {
            MsgLogger.Print($"session '{e.Uuid}' status changed to {e.Status}!");

            if (e.Uuid == _sessionIdentification.Uuid && e.Status == SessionStatus.Offline)
            {
                _asyncChannelReader.Stop();
            }
            else 
            {
                var linkedUuids = SessionService.GetLinkedSessionUuids(e.Uuid, true);

                foreach (var linkedUuid in linkedUuids)
                {
                    var statusUpdate = new SessionStatusUpdate() { Status = e.Status, SessionUuid = linkedUuid };

                    if (_sessionIdentification.Uuid != linkedUuid)
                    {
                        if(!await Send(_channelName, statusUpdate))
                        {
                            MsgLogger.WriteError($"{GetType().Name} - OnSessionStatusChanged", "send status update failed!");
                        }
                    }
                }
            }
        }
        
        private void OnExecCommandAdded(object sender, ExecCommandEventArgs e)
        {
            var execCommand = e.ExecuteCommand;
            var deviceCommand = execCommand?.Command;

            if(deviceCommand !=null &&
               execCommand != null &&
               deviceCommand.Status == ExecCommandStatus.Executed ||
               deviceCommand.Status == ExecCommandStatus.Failed ||
               deviceCommand.Status == ExecCommandStatus.Refused &&
               execCommand.SessionUuid == _sessionIdentification.Uuid)
            {
                lock (_lock)
                {
                    _commandStack.Push(execCommand);
                }
            }

            MsgLogger.WriteDebug($"{GetType().Name} - OnExecCommandAdded", $"slave exec command added name='{e.ExecuteCommand.Command.Name}', status = {e.ExecuteCommand.Command.Status}");
        }

        private void OnExecCommandStatusChanged(object sender, ExecCommandStatusEventArgs e)
        {
            var commandUpdate = e.Status;

            if (commandUpdate!=null &&
                commandUpdate.Status == ExecCommandStatus.Executed ||
                commandUpdate.Status == ExecCommandStatus.Failed ||
                commandUpdate.Status == ExecCommandStatus.Refused &&
                commandUpdate.SessionUuid == _sessionIdentification.Uuid)
            {
                lock (_lock)
                {
                    _commandChangedStack.Push(e.Status);
                }
            }

            MsgLogger.WriteDebug($"{GetType().Name} - OnExecCommandStatusChanged", $"slave exec command changed name='{e.Status.CommandName}', status = {e.Status.Status}");
        }

        private async Task<bool> OnCommandChanged()
        {
            bool result = false;

            try
            {
                MsgLogger.WriteDebug($"{GetType().Name} - OnCommandChanged", $"get exec commands");

                var executeCommands = SessionService.GetExecCommands(_sessionIdentification, new ExecCommandStatus[]
                        { ExecCommandStatus.Executed, ExecCommandStatus.Failed, ExecCommandStatus.Refused});

                result = await CommandsAdded(executeCommands);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - OnCommandChanged", e);
            }

            return result;
        }

        private async Task<bool> CommandsChanged(List<ExecuteCommandStatus> commandsChanged)
        {
            bool result = false;

            try
            {
                if (commandsChanged != null && commandsChanged.Count > 0)
                {
                    try
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - CommandsChanged", $"ExecCommand: Send message to channel '{_channelName}'");

                        if (await Send(_channelName, commandsChanged))
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - CommandsChanged", $"Response to channel '{_channelName}' sent, mark command as sent");

                            result = SetCommandsCommStatus(commandsChanged, ExecCommandCommStatus.SentToAgent);
                        }
                    }
                    catch (Exception ex)
                    {
                        MsgLogger.Exception($"{GetType().Name} - CommandsChanged", ex);
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CommandsChanged", e);
            }

            return result;
        }

        private async Task<bool> CommandsAdded(List<ExecuteCommand> executeCommands)
        {
            bool result = false;

            try
            {
                if (executeCommands != null && executeCommands.Count > 0)
                {
                    try
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - CommandsAdded", $"ExecCommand: Send message to channel '{_channelName}'");

                        if (await Send(_channelName, executeCommands))
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - CommandsAdded", $"Response to channel '{_channelName}' sent, mark command as sent");

                            result = SetCommandsCommStatus(executeCommands, ExecCommandCommStatus.SentToAgent);
                        }
                    }
                    catch (Exception ex)
                    {
                        MsgLogger.Exception($"{GetType().Name} - CommandsAdded", ex);
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CommandsAdded", e);
            }

            return result;
        }

        #endregion

        #region Methods

        public override async Task<bool> ProcessMsg(WsMessage msg)
        {
            bool result = false;

            if (msg.TypeName == typeof(SessionIdentification).FullName)
            {
                _channelName = msg.ChannelName;
                _sessionIdentification = JsonConvert.DeserializeObject<SessionIdentification>(msg.Data);

                const int waitIntervalInMs = 10;

                try
                {
                    _asyncChannelReader.Start();

                    MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"send invite ACK in channel '{msg.ChannelName}' to {Source?.ToString()}");

                    if (await Send(new WsMessageAck()))
                    {
                        var keepAliveManager = new KeepAliveManager(this);

                        keepAliveManager.Start();

                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"process commands channel '{msg.ChannelName}'");

                        result = await OnCommandChanged();

                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"wait for requests channel '{msg.ChannelName}'");

                        RegisterEvents();

                        while (WebSocket.State == WebSocketState.Open && !WebSocket.CloseStatus.HasValue && result &&
                            keepAliveManager.IsRunning && _asyncChannelReader.IsRunning)
                        {
                            var commands = new List<ExecuteCommand>();
                            var commandsChanged = new List<ExecuteCommandStatus>();

                            lock (_lock)
                            {
                                while (_commandStack.TryPop(out var executeCommand))
                                {
                                    commands.Add(executeCommand);
                                }
                            }

                            lock (_lock)
                            {
                                while (_commandChangedStack.TryPop(out var executeCommand))
                                {
                                    commandsChanged.Add(executeCommand);
                                }
                            }

                            result = await CommandsAdded(commands);

                            if (result)
                            {
                                result = await CommandsChanged(commandsChanged);
                            }

                            await Task.Delay(waitIntervalInMs);
                        }

                        UnregisterEvents();

                        keepAliveManager.Stop();
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"send invite ACK in channel '{msg.ChannelName}' to {Source?.ToString()} failed!");
                    }

                    _asyncChannelReader.Stop();
                }
                catch (WebSocketException e)
                {
                    MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"channel '{msg.ChannelName}' exception, error code={e.ErrorCode}, ws error code={e.WebSocketErrorCode}");
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - ProcessMsg", e);
                }

                MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"exit channel '{msg.ChannelName}'");
            }

            return result;
        }

        private void UnregisterEvents()
        {
            SessionService.ExecCommandAdded -= OnExecCommandAdded;
            SessionService.ExecCommandStatusChanged -= OnExecCommandStatusChanged;
            SessionService.SessionStatusChanged -= OnSessionStatusChanged;
        }

        private void RegisterEvents()
        {
            SessionService.ExecCommandAdded += OnExecCommandAdded;
            SessionService.ExecCommandStatusChanged += OnExecCommandStatusChanged;
            SessionService.SessionStatusChanged += OnSessionStatusChanged;
        }

        private bool SetCommandsCommStatus(List<ExecuteCommand> executeCommands, ExecCommandCommStatus status)
        {
            bool result = true;

            var execCommandStatusList = new List<ExecuteCommandStatus>();

            foreach (var executeCommand in executeCommands)
            {
                var execCommandStatus = new ExecuteCommandStatus
                {
                    CommandUuid = executeCommand.CommandUuid,
                    CommandName = executeCommand.Command.Name,
                    SessionUuid = executeCommand.SessionUuid,
                    SerialNumber = executeCommand.SerialNumber
                };

                execCommandStatusList.Add(execCommandStatus);
            }

            if (!SetCommandsCommStatus(execCommandStatusList, status))
            {
                result = false;
            }

            return result;
        }

        private bool SetCommandsCommStatus(List<ExecuteCommandStatus> executeCommandStatusList, ExecCommandCommStatus status)
        {
            bool result = true;

            foreach (var execCommandStatus in executeCommandStatusList)
            {
                execCommandStatus.CommStatus = status;

                if (!SessionService.SetCommandCommStatus(execCommandStatus))
                {
                    MsgLogger.WriteError($"{GetType().Name} - SetCommandsCommStatus", $"change status to {execCommandStatus.Status} failed for command {execCommandStatus.CommandName}!");
                    result = false;
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}

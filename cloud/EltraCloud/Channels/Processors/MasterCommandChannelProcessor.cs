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
    public class MasterCommandChannelProcessor : IChannelProcessor
    {
        #region Private fields

        private readonly object _lock = new object();

        private SessionIdentification _sessionIdentification;
        private string _channelName;
        
        private readonly AsyncChannelReader _asyncChannelReader;
        private readonly Stack<ExecuteCommand> _commandStack;

        #endregion

        #region Constructors

        public MasterCommandChannelProcessor(IPAddress source, WebSocket webSocket, ISessionService sessionService)
            : base(source, webSocket, sessionService)
        {
            _commandStack = new Stack<ExecuteCommand>();

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
                var linkedUuids = SessionService.GetLinkedSessionUuids(e.Uuid, false);

                foreach (var linkedUuid in linkedUuids)
                {
                    var statusUpdate = new SessionStatusUpdate() { Status = e.Status, SessionUuid = linkedUuid };

                    if (_sessionIdentification.Uuid != linkedUuid)
                    {
                        if (!await Send(_channelName, statusUpdate))
                        {
                            MsgLogger.WriteError($"{GetType().Name} - OnSessionStatusChanged", "send status update failed!");
                        }
                    }
                }
            }
        }

        private async Task<bool> OnCommandAdded()
        {
            bool result = false;

            try
            {
                MsgLogger.WriteDebug($"{GetType().Name} - OnCommandAdded", $"pop commands");

                var executeCommands = SessionService.PopCommands(_sessionIdentification, new ExecCommandStatus[] { ExecCommandStatus.Waiting });

                result = await OnCommandAdded(executeCommands);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - OnCommandAdded", e);
            }

            return result;
        }

        private async Task<bool> OnCommandAdded(List<ExecuteCommand> executeCommands)
        {
            bool result = false;

            try
            {
                if (executeCommands != null && executeCommands.Count > 0)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - OnCommandAdded", $"CommandExec: Send message to channel '{_channelName}'");

                    if (await Send(_channelName, executeCommands))
                    {
                        result = SetCommandCommStatus(executeCommands, ExecCommandCommStatus.SentToMaster);
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - OnCommandAdded", e);
            }

            return result;
        }

        private void OnExecCommandAdded(object sender, ExecCommandEventArgs e)
        {
            var executeCommand = e.ExecuteCommand;
            var deviceCommand = executeCommand?.Command;
            var device = deviceCommand?.Device;

            if (deviceCommand!=null &&
                deviceCommand.Status == ExecCommandStatus.Waiting &&
                device != null && device.SessionUuid == _sessionIdentification.Uuid)
            {
                lock (_lock)
                {
                    _commandStack.Push(executeCommand);
                }
            }                
            
            MsgLogger.WriteDebug($"{GetType().Name} - OnExecCommandAdded", $"master exec command added name='{e.ExecuteCommand.Command.Name}', status = {e.ExecuteCommand.Command.Status}, counter = {_commandStack.Count}");
        }

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            SessionService.ExecCommandAdded += OnExecCommandAdded;
            SessionService.SessionStatusChanged += OnSessionStatusChanged;
        }

        private void UnregisterEvents()
        {
            SessionService.ExecCommandAdded -= OnExecCommandAdded;
            SessionService.SessionStatusChanged -= OnSessionStatusChanged;
        }

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
                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"process commands channel '{msg.ChannelName}'");

                        result = await OnCommandAdded();

                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"wait for requests channel '{msg.ChannelName}'");

                        var keepAliveManager = new KeepAliveManager(this);
                        
                        keepAliveManager.Start();

                        RegisterEvents();

                        while (WebSocket.State == WebSocketState.Open && !WebSocket.CloseStatus.HasValue && result && 
                            keepAliveManager.IsRunning && _asyncChannelReader.IsRunning)
                        {
                            var commands = new List<ExecuteCommand>();

                            lock (_lock)
                            {
                                while (_commandStack.TryPop(out var executeCommand))
                                {
                                    commands.Add(executeCommand);
                                }
                            }

                            result = await OnCommandAdded(commands);

                            await Task.Delay(waitIntervalInMs);
                        }

                        UnregisterEvents();

                        keepAliveManager.Stop();

                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"finishing channel '{msg.ChannelName}'");
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"send invite ACK in channel '{msg.ChannelName}' to {Source?.ToString()} failed!");
                    }
                    
                    _asyncChannelReader.Stop();                    
                }
                catch(WebSocketException e)
                {
                    MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"channel '{msg.ChannelName}' exception, error code={e.ErrorCode}, ws error code={e.WebSocketErrorCode}");
                }
                catch(Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - ProcessMsg", e);
                }

                if (!SessionService.SetSessionStatus(_sessionIdentification.Uuid, SessionStatus.Offline))
                {
                    MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"change session (channel '{msg.ChannelName}') status to offline failed!");
                }

                MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"exit channel '{msg.ChannelName}'");
            }

            return result;
        }
        
        private bool SetCommandCommStatus(List<ExecuteCommand> executeCommands, ExecCommandCommStatus status)
        {
            bool result = true;

            foreach (var executeCommand in executeCommands)
            {
                var execCommandStatus = new ExecuteCommandStatus
                {
                    CommandUuid = executeCommand.CommandUuid,
                    CommandName = executeCommand.Command.Name,
                    SessionUuid = executeCommand.SessionUuid,
                    SerialNumber = executeCommand.SerialNumber,

                    CommStatus = status
                };

                if (!SessionService.SetCommandCommStatus(execCommandStatus))
                {
                    MsgLogger.WriteError($"{GetType().Name} - SetCommandCommStatus", $"change command '{execCommandStatus.CommandName}' comm status to {execCommandStatus.CommStatus} failed!");
                    result = false;
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraConnector.Events;
using EltraConnector.Transport.Ws;
using EltraConnector.Transport.Events;
using EltraConnector.Transport.Udp;
using EltraConnector.Transport.Udp.Contracts;
using System.Text.Json;
using EltraConnector.Extensions;
using EltraConnector.Channels;

namespace EltraConnector.Master.Controllers.Commands
{
    class MasterCommandExecutor : WsChannelThread
    {
        #region Private fields

        const string channelName = "Master";

        private readonly MasterChannelControllerAdapter _channelControllerAdapter;
        private bool _stopping;
                
        #endregion

        #region Constructors

        public MasterCommandExecutor(MasterChannelControllerAdapter adapter)
            : base(adapter.ConnectionManager, adapter.Channel.Id + "_CommandExec", channelName, adapter.ChannelId, adapter.User.Identity)
        {
            _channelControllerAdapter = adapter;
        }

        #endregion

        #region Events
                
        public event EventHandler<AgentChannelStatusChangedEventArgs> RemoteChannelStatusChanged;

        #endregion

        #region Events handling

        protected virtual void OnRemoteChannelStatusChanged(AgentChannelStatusChangedEventArgs args)
        {
            RemoteChannelStatusChanged?.Invoke(this, args);
        }

        #endregion

        #region Methods

        protected override async Task Execute()
        {
            const int ReconnectTimeout = 100;
            
            await ConnectToChannel();

            _stopping = false;

            ConnectionManager.MessageReceived += OnMessageReceived;

            while (ShouldRun())
            {
                bool result = await ProcessRequest();

                if (!_stopping)
                {
                    if(!result)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Execute", $"Process request failed, wait {ReconnectTimeout} ms and reconnect...");

                        await Task.Delay(ReconnectTimeout);
                    }

                    await ReconnectToWsChannel();
                }
            }

            ConnectionManager.MessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(object sender, ConnectionMessageEventArgs e)
        {
            if (sender is WsConnection connection && connection.UniqueId == WsChannelId)
            {
                Task.Run(async () =>
                {
                    if (e.Type == MessageType.Data)
                    {
                        var result = await HandleMsgReceived(e.Message);
                    }
                });
            }
            else if (sender is UdpServerConnection udpConnection && udpConnection.UniqueId == WsChannelId)
            {
                var udpRequest = JsonSerializer.Deserialize<UdpRequest>(e.Message);

                if (udpRequest is UdpRequest)
                {
                    if (e.Type == MessageType.Text)
                    {
                        Task.Run(async () =>
                        {
                            var result = await HandleMsgReceived(udpRequest.Data);
                        });
                    }
                }
            }
        }

        private async Task<bool> ProcessWebSocketRequest()
        {
            const int executeIntervalWs = 1;
            bool result = true;

            await ConnectionManager.Receive(WsChannelId, ShouldRun);

            await Task.Delay(executeIntervalWs);

            return result;
        }

        private async Task<bool> HandleMsgReceived(string json)
        {
            bool result = false;

            var start = MsgLogger.BeginTimeMeasure();

            if (WsConnection.IsJson(json))
            {
                result = await ProcessJsonCommand(json);

                if (!result)
                {
                    MsgLogger.WriteError($"{GetType().Name} - HandleMsgReceived", $"Process command '{json}' failed!");
                }
                else
                {
                    MsgLogger.EndTimeMeasure($"{GetType().Name} - HandleMsgReceived", start, $"json message processed, result = {result}");
                }
            }
            else
            {
                if (json == "ACK" || json == "KEEPALIVE")
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - HandleMsgReceived", $"message '{json}' processed, result = {result}");

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - HandleMsgReceived", $"Unknown message {json} received");
                }
            }

            return result;
        }

        private ExecuteCommand ParseExecuteCommand(string json)
        {
            ExecuteCommand result = null;
            
            try
            {
                var executeCommand = JsonSerializer.Deserialize<ExecuteCommand>(json);

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
                var executeCommandStatus = JsonSerializer.Deserialize<ExecuteCommandStatus>(json);

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

        private List<ExecuteCommand> ParseExecuteCommandSet(string json)
        {
            List<ExecuteCommand> result = null;
            
            try
            {
                var executeCommands = JsonSerializer.Deserialize<List<ExecuteCommand>>(json);

                if(executeCommands != null && executeCommands.Count > 0)
                {
                    result = new List<ExecuteCommand>();

                    foreach(var executeCommand in executeCommands)
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

        private List<ExecuteCommandStatus> ParseExecuteCommandStatusSet(string json)
        {
            List<ExecuteCommandStatus> result = null;

            try
            {
                var executeCommandStatusList = JsonSerializer.Deserialize<List<ExecuteCommandStatus>>(json);

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

        private ChannelStatusUpdate ParseChannelStatusUpdate(string json)
        {
            ChannelStatusUpdate result = null;
            
            try
            {
                var channelStatusUpdate = JsonSerializer.Deserialize<ChannelStatusUpdate>(json);

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

        private ChannelIdentification ParseChannelIdentification(string json)
        {
            ChannelIdentification result = null;

            try
            {
                var channelIdentification = JsonSerializer.Deserialize<ChannelIdentification>(json);

                if (channelIdentification != null)
                {
                    result = channelIdentification;
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        private async Task<bool> ProcessJsonCommand(string json)
        {
            bool result = false;

            try
            {
                var start = MsgLogger.BeginTimeMeasure();

                var executeCommands = ParseExecuteCommandSet(json);

                if (executeCommands != null && executeCommands.Count > 0)
                {
                    int processedCommands = await _channelControllerAdapter.ExecuteCommands(executeCommands);

                    MsgLogger.WriteFlow($"{GetType().Name} - ProcessJsonCommand", $"executed: received commends = {executeCommands.Count}, processed commands = {processedCommands}");

                    result = true;
                }
                else
                {
                    var executeCommand = ParseExecuteCommand(json);

                    if (executeCommand != null)
                    {
                        int processedCommands = await _channelControllerAdapter.ExecuteCommands(new List<ExecuteCommand>() { executeCommand });

                        MsgLogger.WriteFlow($"{GetType().Name} - ProcessJsonCommand", $"executed: received commends = 1, processed commands = {processedCommands}");

                        result = true;
                    }
                    else
                    {
                        var executeCommandStatusSet = ParseExecuteCommandStatusSet(json);

                        if (executeCommandStatusSet != null)
                        {
                            foreach(var executeCommandStatus in executeCommandStatusSet)
                            {
                                MsgLogger.WriteFlow($"{GetType().Name} - ProcessJsonCommand", $"command {executeCommandStatus.CommandName} status changed = {executeCommandStatus.Status}");
                            }

                            result = true;
                        }
                        else
                        {
                            var executeCommandStatus = ParseExecuteCommandStatus(json);

                            if (executeCommandStatus != null)
                            {
                                MsgLogger.WriteFlow($"{GetType().Name} - ProcessJsonCommand", $"command {executeCommandStatus.CommandName} status changed = {executeCommandStatus.Status}");

                                result = true;
                            }
                            else
                            {
                                var channelStatusUpdate = ParseChannelStatusUpdate(json);

                                if (channelStatusUpdate != null)
                                {
                                    if (channelStatusUpdate.ChannelId != _channelControllerAdapter.Channel.Id)
                                    {
                                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessJsonCommand", $"session {channelStatusUpdate.ChannelId}, status changed to {channelStatusUpdate.Status}");

                                        OnRemoteChannelStatusChanged(new AgentChannelStatusChangedEventArgs() { Id = channelStatusUpdate.ChannelId, Status = channelStatusUpdate.Status });
                                    }

                                    result = true;
                                }
                                else
                                {
                                    var channelIdentification = ParseChannelIdentification(json);

                                    if (channelIdentification != null)
                                    {
                                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessJsonCommand", $"identification, channel = {channelIdentification.Id} node id = {channelIdentification.NodeId}");
                                        result = true;
                                    }
                                    else
                                    {
                                        MsgLogger.WriteError($"{GetType().Name} - ProcessJsonCommand", $"Unknown message {json} received");
                                    }
                                }
                            }
                        }
                    }
                }

                MsgLogger.EndTimeMeasure($"{GetType().Name} - ProcessJsonCommand", start, $"commands processed");
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ProcessJsonCommand", e);
            }

            return result;
        }

        private async Task<bool> ProcessRestRequest()
        {
            const int executeIntervalRest = 10;
            
            bool result = await _channelControllerAdapter.ExecuteCommands();

            await Task.Delay(executeIntervalRest);

            return result;
        }

        private async Task<bool> ProcessRequest()
        {
            bool result = false;
            
            if (ConnectionManager != null && ConnectionManager.IsConnected(WsChannelId))
            {
                result = await ProcessWebSocketRequest();
            }
            else if (!_stopping)
            {
                result = await ProcessRestRequest();
            }

            return result;
        }

        #endregion
    }
}

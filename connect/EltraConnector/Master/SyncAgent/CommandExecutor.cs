using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Threads;
using EltraConnector.Controllers;
using EltraConnector.Events;
using EltraConnector.Transport.Ws;
using EltraConnector.Transport.Udp.Response;
using EltraConnector.Transport.Udp;
using EltraConnector.Transport.Ws.Events;
using EltraConnector.Transport.Udp.Contracts;
using EltraConnector.Transport.Ws.Interfaces;
using EltraConnector.Master.Controllers;

namespace EltraConnector.SyncAgent
{
    class CommandExecutor : EltraThread
    {
        #region Private fields

        private readonly MasterChannelControllerAdapter _channelControllerAdapter;
        private readonly EltraUdpServer _udpServer;
        private bool _stopping;
        private string _wsChannelId;
        
        #endregion

        #region Constructors

        public CommandExecutor(MasterChannelControllerAdapter adapter, EltraUdpServer udpServer)
        {
            _udpServer = udpServer; 
            _channelControllerAdapter = adapter;
        }

        #endregion

        #region Events

        public event EventHandler<SignInRequestEventArgs> SignInRequested;
        public event EventHandler<AgentChannelStatusChangedEventArgs> RemoteChannelStatusChanged;

        #endregion

        #region Events handling

        protected virtual void OnRemoteChannelStatusChanged(AgentChannelStatusChangedEventArgs args)
        {
            RemoteChannelStatusChanged?.Invoke(this, args);
        }

        private bool OnSignInRequested()
        {
            var args = new SignInRequestEventArgs();

            SignInRequested?.Invoke(this, args);

            return args.SignInResult;
        }

        #endregion

        #region Properties

        private IConnectionManager WsConnectionManager => _channelControllerAdapter.WsConnectionManager;

        #endregion

        #region Methods

        private async Task SendSessionIdentyfication(string commandExecUuid)
        {
            if (WsConnectionManager != null && WsConnectionManager.IsConnected(commandExecUuid))
            {
                var sessionIdent = new ChannelIdentification() { Id = _channelControllerAdapter.Channel.Id };
                
                await WsConnectionManager.Send(commandExecUuid, _channelControllerAdapter.User.Identity, sessionIdent);
            }
        }

        private async Task Connect(string channelId, string channelName)
        {
            if (WsConnectionManager != null && WsConnectionManager.CanConnect(channelId))
            {
                if (OnSignInRequested())
                {
                    if (await WsConnectionManager.Connect(channelId, channelName))
                    {
                        await SendSessionIdentyfication(channelId);
                    }
                }
            }
        }

        protected override async Task Execute()
        {
            const int ReconnectTimeout = 100;
            
            _wsChannelId = _channelControllerAdapter.Channel.Id + "_CommandExec";

            string channelName = "CommandsExecution";
            
            await Connect(_wsChannelId, channelName);

            _stopping = false;

            _udpServer.MessageReceived += OnUdpServerMessageReceived;

            WsConnectionManager.MessageReceived += OnWsMessageReceived;

            while (ShouldRun())
            {
                bool result = await ProcessRequest(_wsChannelId);

                if (!_stopping)
                {
                    if(!result)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Execute", $"Process request failed, wait {ReconnectTimeout} ms and reconnect...");

                        await Task.Delay(ReconnectTimeout);
                    }

                    await Connect(_wsChannelId, channelName);
                }
            }
        }

        private void OnWsMessageReceived(object sender, WsConnectionMessageEventArgs e)
        {
            if (sender is WsConnection connection && connection.UniqueId == _wsChannelId)
            {
                Task.Run(async () =>
                {
                    if (e.Type == WsMessageType.Data)
                    {
                        var result = await HandleMsgReceived(e.Message);
                    }
                });
            }
        }

        private void OnUdpServerMessageReceived(object sender, ReceiveResponse e)
        {
            e.Handled = true;

            Task.Run(async () => {

                if (WsConnection.IsJson(e.Text))
                {
                    try
                    {
                        var udpRequest = System.Text.Json.JsonSerializer.Deserialize<UdpRequest>(e.Text);

                        if (udpRequest != null)
                        {
                            await _udpServer.Send(e.Endpoint, _channelControllerAdapter.User.Identity, new UdpAckRequest());

                            var result = await HandleMsgReceived(udpRequest.Data);
                        }
                    }
                    catch(Exception ex)
                    {
                        MsgLogger.Exception($"{GetType().Name} - OnUdpServerMessageReceived", ex);
                    }
                }
            });            
        }

        private async Task<bool> ProcessWebSocketRequest(string sessionUuid)
        {
            const int executeIntervalWs = 1;
            bool result = true;

            try
            {
                string msg = string.Empty;
                do
                {
                    msg = await WsConnectionManager.Receive(sessionUuid);

                    if (ShouldRun())
                    {
                        await Task.Delay(executeIntervalWs);
                    }
                }
                while ((msg == "KEEPALIVE" || msg == "ACK") && ShouldRun());
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ProcessWebSocketRequest", e);
                result = false;
            }

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
                    MsgLogger.EndTimeMeasure($"{GetType().Name} - HandleMsgReceived", start, $"message '{json}' processed, result = {result}");

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - HandleMsgReceived", $"Unknown message {json} received");
                }
            }

            return result;
        }

        private async Task<bool> ProcessJsonCommand(string json)
        {
            bool result = false;

            try
            {
                var start = MsgLogger.BeginTimeMeasure();
                List<ExecuteCommand> executeCommands = null;

                try
                {
                    executeCommands = System.Text.Json.JsonSerializer.Deserialize<List<ExecuteCommand>>(json);
                }
                catch(Exception)
                {
                }

                if (executeCommands != null)
                {
                    int processedCommands = await _channelControllerAdapter.ExecuteCommands(executeCommands);

                    MsgLogger.WriteFlow($"{GetType().Name} - ProcessWebSocketRequest", $"executed: received commends = {executeCommands.Count}, processed commands = {processedCommands}");

                    result = true;
                }
                else
                {
                    ChannelStatusUpdate channelStatusUpdate = null;
                    
                    try
                    {
                        channelStatusUpdate = System.Text.Json.JsonSerializer.Deserialize<ChannelStatusUpdate>(json);
                    }
                    catch (Exception)
                    {
                    }

                    if (channelStatusUpdate != null)
                    {
                        if (channelStatusUpdate.ChannelId != _channelControllerAdapter.Channel.Id)
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"session {channelStatusUpdate.ChannelId}, status changed to {channelStatusUpdate.Status}");

                            OnRemoteChannelStatusChanged(new AgentChannelStatusChangedEventArgs() { Id = channelStatusUpdate.ChannelId, Status = channelStatusUpdate.Status });
                        }

                        result = true;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Execute", $"Unknown message {json} received");
                    }
                }

                MsgLogger.EndTimeMeasure($"{GetType().Name} - ProcessJsonCommand", start, $"commands processed");
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Execute", e);
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

        private async Task<bool> ProcessRequest(string channelId)
        {
            bool result = false;
            
            if (WsConnectionManager != null && WsConnectionManager.IsConnected(channelId))
            {
                result = await ProcessWebSocketRequest(channelId);
            }
            else if (!_stopping)
            {
                result = await ProcessRestRequest();
            }

            return result;
        }

        public override bool Stop()
        {
            _stopping = true;
            
            Task.Run(async () => { await WsConnectionManager.Disconnect(_wsChannelId); }).GetAwaiter().GetResult();

            bool result = base.Stop();

            return result;
        }

        #endregion
    }
}

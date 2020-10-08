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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EltraConnector.SyncAgent
{
    class CommandExecutor : EltraThread
    {
        #region Private fields

        private readonly DeviceChannelControllerAdapter _channelControllerAdapter;
        private bool _stopping;
        private string _wsChannelId;

        #endregion

        #region Constructors

        public CommandExecutor(DeviceChannelControllerAdapter adapter)
        {
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

        #region Methods

        private async Task SendSessionIdentyfication(string commandExecUuid)
        {
            var wsConnectionManager = _channelControllerAdapter?.WsConnectionManager;

            if (wsConnectionManager != null && wsConnectionManager.IsConnected(commandExecUuid))
            {
                var sessionIdent = new ChannelIdentification() { Id = _channelControllerAdapter.Channel.Id };
                
                await wsConnectionManager.Send(commandExecUuid, _channelControllerAdapter.User.Identity, sessionIdent);
            }
        }

        private void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var msg = errorArgs.ErrorContext.Error.Message;

            MsgLogger.WriteDebug($"{GetType().Name} - HandleDeserializationError", msg);

            errorArgs.ErrorContext.Handled = true;
        }

        private async Task Connect(string sessionUuid, string channelName)
        {
            var wsConnectionManager = _channelControllerAdapter?.WsConnectionManager;

            if (wsConnectionManager != null)
            {
                if (wsConnectionManager.CanConnect(sessionUuid))
                {
                    if (OnSignInRequested())
                    {
                        if (await wsConnectionManager.Connect(sessionUuid, channelName))
                        {
                            await SendSessionIdentyfication(sessionUuid);
                        }
                    }
                }
            }
        }

        protected override async Task Execute()
        {
            const int ReconnectTimeout = 5000;
            
            _wsChannelId = _channelControllerAdapter.Channel.Id + "_CommandExec";

            string channelName = "CommandsExecution";
            
            await Connect(_wsChannelId, channelName);

            _stopping = false;

            while (ShouldRun())
            {
                bool result = await ProcessRequest(_wsChannelId);

                if (!_stopping)
                {
                    if(!result)
                    {
                        await Task.Delay(ReconnectTimeout);
                    }

                    await Connect(_wsChannelId, channelName);
                }
            }
        }

        private async Task<bool> ProcessWebSocketRequest(string sessionUuid)
        {
            const int executeIntervalWs = 1;
            bool result = false;

            try
            {
                var wsConnectionManager = _channelControllerAdapter?.WsConnectionManager;
                var json = await wsConnectionManager.Receive(sessionUuid);

                if (WsConnection.IsJson(json))
                {
                    var executeCommands = JsonConvert.DeserializeObject<List<ExecuteCommand>>(json, new JsonSerializerSettings
                    {
                        Error = HandleDeserializationError
                    });

                    if (executeCommands != null)
                    {
                        int processedCommands = await _channelControllerAdapter.ExecuteCommands(executeCommands);

                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessWebSocketRequest", $"executed: received commends = {executeCommands.Count}, processed commands = {processedCommands}");

                        result = true;
                    }
                    else
                    {
                        var channelStatusUpdate = JsonConvert.DeserializeObject<ChannelStatusUpdate>(json, new JsonSerializerSettings
                        {
                            Error = HandleDeserializationError
                        });

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
                            MsgLogger.WriteLine($"{GetType().Name} - Execute - Unknown message received");
                        }
                    }
                }
                else
                {
                    MsgLogger.WriteLine($"{GetType().Name} - Execute - message received");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Execute", e);
            }

            await Task.Delay(executeIntervalWs);

            return result;
        }

        private async Task<bool> ProcessRestRequest()
        {
            const int executeIntervalRest = 100;
            
            bool result = await _channelControllerAdapter.ExecuteCommands();

            await Task.Delay(executeIntervalRest);

            return result;
        }

        private async Task<bool> ProcessRequest(string channelId)
        {
            bool result = false;
            var wsConnectionManager = _channelControllerAdapter?.WsConnectionManager;

            if (wsConnectionManager != null && wsConnectionManager.IsConnected(channelId))
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
            var wsConnectionManager = _channelControllerAdapter?.WsConnectionManager;

            Task.Run(async () => { await wsConnectionManager.Disconnect(_wsChannelId); }).GetAwaiter().GetResult();

            bool result = base.Stop();

            return result;
        }

        #endregion
    }
}

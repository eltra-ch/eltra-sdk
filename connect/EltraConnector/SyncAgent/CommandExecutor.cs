using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Sessions;
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

        private readonly DeviceSessionControllerAdapter _sessionControllerAdapter;
        private readonly WsConnectionManager _wsConnectionManager;
        private bool _stopping;
        
        #endregion

        #region Constructors

        public CommandExecutor(DeviceSessionControllerAdapter adapter)
        {
            _sessionControllerAdapter = adapter;
            _wsConnectionManager = new WsConnectionManager() { HostUrl = _sessionControllerAdapter.Url };
        }

        #endregion

        #region Events

        public event EventHandler<SessionStatusChangedEventArgs> RemoteSessionStatusChanged;

        protected virtual void OnRemoteSessionStatusChanged(SessionStatusChangedEventArgs args)
        {
            RemoteSessionStatusChanged?.Invoke(this, args);
        }

        #endregion

        #region Methods

        private async Task SendSessionIdentyfication(string commandExecUuid)
        {
            if(_wsConnectionManager.IsConnected(commandExecUuid))
            {
                var sessionIdent = new SessionIdentification() { Uuid = _sessionControllerAdapter.Session.Uuid, AuthData = _sessionControllerAdapter.User.AuthData };
                
                await _wsConnectionManager.Send(commandExecUuid, sessionIdent);
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
            if (_wsConnectionManager != null)
            {
                if (_wsConnectionManager.CanConnect(sessionUuid))
                {
                    if (await _wsConnectionManager.Connect(sessionUuid, channelName))
                    {
                        await SendSessionIdentyfication(sessionUuid);
                    }
                }
            }
        }

        protected override async Task Execute()
        {
            var sessionUuid = _sessionControllerAdapter.Session.Uuid + "_CommandExec";
            string channelName = "CommandsExecution";

            await Connect(sessionUuid, channelName);

            _stopping = false;

            while (ShouldRun())
            {
                await ProcessRequest(sessionUuid);

                if (!_stopping)
                {
                    await Connect(sessionUuid, channelName);
                }
            }
        }

        private async Task ProcessWebSocketRequest(string sessionUuid)
        {
            const int executeIntervalWs = 1;

            try
            {
                var json = await _wsConnectionManager.Receive(sessionUuid);

                if (WsConnection.IsJson(json))
                {
                    var executeCommands = JsonConvert.DeserializeObject<List<ExecuteCommand>>(json, new JsonSerializerSettings
                    {
                        Error = HandleDeserializationError
                    });

                    if (executeCommands != null)
                    {
                        int processedCommands = await _sessionControllerAdapter.ExecuteCommands(executeCommands);

                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessWebSocketRequest", $"executed: received commends = {executeCommands.Count}, processed commands = {processedCommands}");
                    }
                    else
                    {
                        var sessionStatusUpdate = JsonConvert.DeserializeObject<SessionStatusUpdate>(json, new JsonSerializerSettings
                        {
                            Error = HandleDeserializationError
                        });

                        if (sessionStatusUpdate != null)
                        {
                            if (sessionStatusUpdate.SessionUuid != _sessionControllerAdapter.Session.Uuid)
                            {
                                MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"session {sessionStatusUpdate.SessionUuid}, status changed to {sessionStatusUpdate.Status}");

                                OnRemoteSessionStatusChanged(new SessionStatusChangedEventArgs() { Uuid = sessionStatusUpdate.SessionUuid, Status = sessionStatusUpdate.Status });
                            }
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
        }

        private async Task ProcessRestRequest()
        {
            const int executeIntervalRest = 100;

            await _sessionControllerAdapter.ExecuteCommands();

            await Task.Delay(executeIntervalRest);
        }

        private async Task ProcessRequest(string sessionUuid)
        {   
            if (_wsConnectionManager != null && _wsConnectionManager.IsConnected(sessionUuid))
            {
                await ProcessWebSocketRequest(sessionUuid);
            }
            else if (!_stopping)
            {
                await ProcessRestRequest();
            }
        }

        public override bool Stop()
        {
            _stopping = true;

            Task.Run(async () => { await _wsConnectionManager.DisconnectAll(); }).GetAwaiter().GetResult();

            bool result = base.Stop();

            return result;
        }

        #endregion
    }
}

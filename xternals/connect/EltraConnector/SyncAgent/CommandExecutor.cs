using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Sessions;
using EltraCommon.Logger;
using EltraCommon.Threads;
using EltraConnector.Controllers;
using EltraConnector.Events;
using EltraConnector.Ws;
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
                var sessionIdent = new SessionIdentification() { Uuid = _sessionControllerAdapter.Session.Uuid };
                
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

            var commandExecUuid = _sessionControllerAdapter.Session.Uuid + "_CommandExec";
            string wsChannelName = "CommandsExecution";

            if (_wsConnectionManager.CanConnect(commandExecUuid))
            {
                if(await _wsConnectionManager.Connect(commandExecUuid, wsChannelName))
                {
                    await SendSessionIdentyfication(commandExecUuid);
                }
            }

            _stopping = false;

            while (ShouldRun())
            {
                if(_wsConnectionManager.IsConnected(commandExecUuid))
                {
                    var json = await _wsConnectionManager.Receive(commandExecUuid);

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
                                await _sessionControllerAdapter.ExecuteCommands(executeCommands);
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
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MsgLogger.Exception($"{GetType().Name} - Execute", e);
                    }

                    await Task.Delay(executeIntervalWs);
                }
                else if(!_stopping)
                {
                    await _sessionControllerAdapter.ExecuteCommands();
                    
                    await Task.Delay(executeIntervalRest);
                }

                if (_wsConnectionManager.CanConnect(commandExecUuid) && !_stopping)
                {
                    if(await _wsConnectionManager.Connect(commandExecUuid, wsChannelName))
                    {
                        await SendSessionIdentyfication(commandExecUuid);
                    }
                }                
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

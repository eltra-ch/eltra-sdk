using System;
using System.Threading;
using System.Threading.Tasks;
using EltraConnector.Controllers;
using EltraCommon.Logger;
using EltraConnector.Ws;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Threads;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EltraConnector.UserAgent
{
    class ParameterUpdateManager : EltraThread
    {
        #region Private fields

        private readonly UserSessionControllerAdapter _sessionAdapter;        
        private readonly WsConnectionManager _wsConnectionManager;
        private string _wsChannelName;
        private string _commandExecUuid;

        #endregion

        #region Constructors

        public ParameterUpdateManager(UserSessionControllerAdapter sessionAdapter)
        {
            _wsConnectionManager = new WsConnectionManager() { HostUrl = sessionAdapter.Url };

            _sessionAdapter = sessionAdapter;
            _commandExecUuid = _sessionAdapter.Uuid + "_ParameterUpdate";
            _wsChannelName = "ParameterUpdate";
        }

        #endregion
        
        #region Events

        public event EventHandler<ParameterChangedEventArgs> ParameterChanged;

        protected virtual void OnParameterChanged(ParameterChangedEventArgs e)
        {
            ParameterChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public override bool Stop()
        {
            Task.Run(async ()=> await CloseWsChannel(_commandExecUuid));

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
            const int executeIntervalWs = 10;
            
            await CreateWsChannel(_commandExecUuid, _wsChannelName);

            while (ShouldRun())
            {
                try
                {
                    if (_wsConnectionManager.IsConnected(_commandExecUuid))
                    {
                        var json = await _wsConnectionManager.Receive(_commandExecUuid);

                        _ = Task.Run(() =>
                        {
                            if (WsConnection.IsJson(json))
                            {
                                var parameterEntry = JsonConvert.DeserializeObject<Parameter>(json, new JsonSerializerSettings
                                {
                                    Error = HandleDeserializationError
                                });

                                if (parameterEntry != null)
                                {
                                    OnParameterChanged(new ParameterChangedEventArgs(parameterEntry, null, parameterEntry.ActualValue));
                                }
                            }
                        });
                       
                    }                    
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Execute", e);
                }

                if (ShouldRun() && !_wsConnectionManager.IsConnected(_commandExecUuid))
                {
                    await CreateWsChannel(_commandExecUuid, _wsChannelName);

                    await Task.Delay(executeIntervalWs);
                }
            }

            await CloseWsChannel(_commandExecUuid);
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

        private async Task CloseWsChannel(string commandExecUuid)
        {
            if (_wsConnectionManager.IsConnected(commandExecUuid))
            {
                await _wsConnectionManager.Disconnect(commandExecUuid);
            }
        }

        #endregion
    }
}

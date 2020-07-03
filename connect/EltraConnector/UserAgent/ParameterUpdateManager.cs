using System;
using System.Threading.Tasks;
using EltraConnector.Controllers;
using EltraCommon.Logger;
using EltraConnector.Transport.Ws;
using EltraCommon.Contracts.Sessions;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Threads;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

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

        private async Task<bool> SendSessionIdentyfication(string commandExecUuid)
        {
            bool result = false;

            if (_wsConnectionManager.IsConnected(commandExecUuid))
            {
                var sessionIdent = new SessionIdentification() { Uuid = _sessionAdapter.Uuid };

                result = await _wsConnectionManager.Send(commandExecUuid, sessionIdent);
            }

            return result;
        }

        private void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var msg = errorArgs.ErrorContext.Error.Message;

            MsgLogger.WriteDebug($"{GetType().Name} - HandleDeserializationError", msg);

            errorArgs.ErrorContext.Handled = true;
        }

        protected override async Task Execute()
        {
            const int executeIntervalWs = 1;

            var parameterChangedTasks = new List<Task>();

            await CreateWsChannel(_commandExecUuid, _wsChannelName);
            
            while (ShouldRun())
            {
                try
                {
                    if (_wsConnectionManager.IsConnected(_commandExecUuid))
                    {
                        var json = await _wsConnectionManager.Receive(_commandExecUuid);

                        var parameterChangedTask = Task.Run(() =>
                        {
                            if (WsConnection.IsJson(json))
                            {
                                var parameterSet = JsonConvert.DeserializeObject<ParameterSet>(json, new JsonSerializerSettings
                                {
                                    Error = HandleDeserializationError
                                });

                                if (parameterSet != null && parameterSet.Count > 0)
                                {
                                    foreach(var parameterEntry in parameterSet.ParameterList)
                                    {
                                        OnParameterChanged(new ParameterChangedEventArgs(parameterEntry, null, parameterEntry.ActualValue));
                                    }
                                }
                                else
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
                            }
                        });

                        parameterChangedTasks.Add(parameterChangedTask);
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

            foreach(var parameterChangedTask in parameterChangedTasks)
            {
                parameterChangedTask.Wait();
            }

            await CloseWsChannel(_commandExecUuid);
        }

        private async Task<bool> CreateWsChannel(string commandExecUuid, string wsChannelName)
        {
            bool result = false;

            if (_wsConnectionManager.CanConnect(commandExecUuid))
            {
                if (await _wsConnectionManager.Connect(commandExecUuid, wsChannelName))
                {
                    result = await SendSessionIdentyfication(commandExecUuid);
                }
            }

            return result;
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

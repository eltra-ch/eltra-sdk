using System;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraConnector.Transport.Ws;
using EltraCommon.Contracts.Channels;
using EltraCommon.Threads;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using EltraConnector.Controllers.Base;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Parameters.Events;
using EltraConnector.Events;

namespace EltraConnector.UserAgent
{
    class ParameterUpdateManager : EltraThread
    {
        #region Private fields

        private readonly ChannelControllerAdapter _channelAdapter;        
        private readonly WsConnectionManager _wsConnectionManager;
        private string _wsChannelName;
        private string _wsChannelId;

        #endregion

        #region Constructors

        public ParameterUpdateManager(ChannelControllerAdapter channelAdapter)
        {
            _wsConnectionManager = channelAdapter.WsConnectionManager;

            _channelAdapter = channelAdapter;
            _wsChannelId = _channelAdapter.ChannelId + "_ParameterUpdate";
            _wsChannelName = "ParameterUpdate";
        }

        public ParameterUpdateManager(ChannelControllerAdapter channelAdapter, int nodeId)
        {
            _wsConnectionManager = channelAdapter.WsConnectionManager;

            _channelAdapter = channelAdapter;
            _wsChannelId = _channelAdapter.ChannelId + $"_ParameterUpdate_{nodeId}";
            _wsChannelName = "ParameterUpdate";
        }

        #endregion

        #region Events

        public event EventHandler<ParameterValueChangedEventArgs> ParameterValueChanged;

        public event EventHandler<SignInRequestEventArgs> SignInRequested;

        #endregion

        #region Events handling

        protected virtual void OnParameterValueChanged(ParameterValueChangedEventArgs e)
        {
            ParameterValueChanged?.Invoke(this, e);
        }

        private bool OnSignInRequested()
        {
            var args = new SignInRequestEventArgs();

            SignInRequested?.Invoke(this, args);

            return args.SignInResult;
        }

        #endregion

        #region Methods

        public override bool Stop()
        {
            Task.Run(async ()=> await CloseWsChannel(_wsChannelId));

            return base.Stop();
        }

        private async Task<bool> SendSessionIdentyfication(string commandExecUuid)
        {
            bool result = false;

            if (_wsConnectionManager.IsConnected(commandExecUuid))
            {
                var sessionIdent = new ChannelIdentification() { Id = _channelAdapter.ChannelId };

                result = await _wsConnectionManager.Send(commandExecUuid, _channelAdapter.User.Identity, sessionIdent);
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

            await CreateWsChannel(_wsChannelId, _wsChannelName);
            
            while (ShouldRun())
            {
                try
                {
                    if (_wsConnectionManager.IsConnected(_wsChannelId))
                    {
                        var json = await _wsConnectionManager.Receive(_wsChannelId);

                        var parameterChangedTask = Task.Run(() =>
                        {
                            if (WsConnection.IsJson(json))
                            {
                                var parameterSet = JsonConvert.DeserializeObject<ParameterValueUpdateSet>(json, new JsonSerializerSettings
                                {
                                    Error = HandleDeserializationError
                                });

                                if (parameterSet != null && parameterSet.Count > 0)
                                {
                                    foreach(var parameterEntry in parameterSet.Items)
                                    {
                                        OnParameterValueChanged(new ParameterValueChangedEventArgs(parameterEntry.NodeId, 
                                                                                                   parameterEntry.Index, 
                                                                                                   parameterEntry.SubIndex, 
                                                                                                   parameterEntry.ParameterValue));
                                    }
                                }
                                else
                                {
                                    var parameterEntry = JsonConvert.DeserializeObject<ParameterValueUpdate>(json, new JsonSerializerSettings
                                    {
                                        Error = HandleDeserializationError
                                    });

                                    if (parameterEntry != null)
                                    {
                                        OnParameterValueChanged(new ParameterValueChangedEventArgs(parameterEntry.NodeId,
                                                                                                   parameterEntry.Index,
                                                                                                   parameterEntry.SubIndex, parameterEntry.ParameterValue));
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

                if (ShouldRun() && !_wsConnectionManager.IsConnected(_wsChannelId))
                {
                    await CreateWsChannel(_wsChannelId, _wsChannelName);

                    await Task.Delay(executeIntervalWs);
                }
            }

            foreach(var parameterChangedTask in parameterChangedTasks)
            {
                parameterChangedTask.Wait();
            }

            await CloseWsChannel(_wsChannelId);
        }

        private async Task<bool> CreateWsChannel(string commandExecUuid, string wsChannelName)
        {
            bool result = false;

            if (_wsConnectionManager.CanConnect(commandExecUuid))
            {
                if (OnSignInRequested())
                {
                    if (await _wsConnectionManager.Connect(commandExecUuid, wsChannelName))
                    {
                        result = await SendSessionIdentyfication(commandExecUuid);
                    }
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

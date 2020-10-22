using System;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraConnector.Transport.Ws;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using EltraConnector.Controllers.Base;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Parameters.Events;
using EltraConnector.Channels.Events;

namespace EltraConnector.Channels
{
    class ParameterUpdateManager : WsChannelThread
    {
        #region Private fields

        const string ChannelName = "ParameterUpdate";

        private readonly ChannelControllerAdapter _channelAdapter;        
        
        #endregion

        #region Constructors

        public ParameterUpdateManager(ChannelControllerAdapter channelAdapter)
            : base(channelAdapter.WsConnectionManager, channelAdapter.ChannelId + "_ParameterUpdate", ChannelName,
                  channelAdapter.ChannelId, 0, channelAdapter.User.Identity)
        {
            _channelAdapter = channelAdapter;
        }

        public ParameterUpdateManager(ChannelControllerAdapter channelAdapter, int nodeId)
            : base(channelAdapter.WsConnectionManager, channelAdapter.ChannelId + $"_ParameterUpdate_{nodeId}", ChannelName,
                  channelAdapter.ChannelId, nodeId, channelAdapter.User.Identity)
        {
            _channelAdapter = channelAdapter;
        }

        #endregion

        #region Events

        public event EventHandler<ParameterValueChangedEventArgs> ParameterValueChanged;

        #endregion

        #region Events handling

        protected virtual void OnParameterValueChanged(ParameterValueChangedEventArgs e)
        {
            ParameterValueChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        private void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var msg = errorArgs.ErrorContext.Error.Message;

            MsgLogger.WriteDebug($"{GetType().Name} - HandleDeserializationError", msg);

            errorArgs.ErrorContext.Handled = true;
        }

        protected override async Task Execute()
        {
            const int executeIntervalWs = 1;

            Status = WsChannelStatus.Starting;

            var parameterChangedTasks = new List<Task>();

            await ConnectToWsChannel();

            while (ShouldRun())
            {
                try
                {
                    if (WsConnectionManager.IsConnected(WsChannelId))
                    {
                        var json = await WsConnectionManager.Receive(WsChannelId);

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
                                    foreach (var parameterEntry in parameterSet.Items)
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

                if (ShouldRun())
                {
                    await ReconnectToWsChannel();
                    
                    await Task.Delay(executeIntervalWs);
                }
            }

            Task.WaitAll(parameterChangedTasks.ToArray());

            await DisconnectFromWsChannel();            
        }

        #endregion
    }
}

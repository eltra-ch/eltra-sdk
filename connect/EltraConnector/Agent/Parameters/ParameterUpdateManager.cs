using System;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraConnector.Transport.Ws;
using System.Collections.Generic;
using EltraConnector.Controllers.Base;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Parameters.Events;
using EltraConnector.Channels.Events;
using EltraConnector.Transport.Events;
using EltraConnector.Channels;
using EltraCommon.Extensions;

namespace EltraConnector.Agent.Parameters
{
    class ParameterUpdateManager : WsChannelThread
    {
        #region Private fields

        const string ChannelName = "ParameterUpdate";

        private readonly ChannelControllerAdapter _channelAdapter;
        private readonly List<Task> _parameterChangedTasks;

        #endregion

        #region Constructors

        public ParameterUpdateManager(ChannelControllerAdapter channelAdapter, string channelName, string channelId)
            : base(channelAdapter.ConnectionManager, channelId, channelName,
                  channelAdapter.ChannelId, 0, channelAdapter.User.Identity)
        {
            _channelAdapter = channelAdapter;
            _parameterChangedTasks = new List<Task>();
        }

        public ParameterUpdateManager(ChannelControllerAdapter channelAdapter)
            : base(channelAdapter.ConnectionManager, channelAdapter.ChannelId + "_ParameterUpdate", ChannelName,
                  channelAdapter.ChannelId, 0, channelAdapter.User.Identity)
        {
            _channelAdapter = channelAdapter;
            _parameterChangedTasks = new List<Task>();
        }

        public ParameterUpdateManager(ChannelControllerAdapter channelAdapter, int nodeId, string channelName, string channelId)
            : base(channelAdapter.ConnectionManager, channelId, channelName,
                  channelAdapter.ChannelId, nodeId, channelAdapter.User.Identity)
        {
            _channelAdapter = channelAdapter;
            _parameterChangedTasks = new List<Task>();
        }

        public ParameterUpdateManager(ChannelControllerAdapter channelAdapter, int nodeId)
            : base(channelAdapter.ConnectionManager, channelAdapter.ChannelId + $"_ParameterUpdate_{nodeId}", ChannelName,
                  channelAdapter.ChannelId, nodeId, channelAdapter.User.Identity)
        {
            _channelAdapter = channelAdapter;
            _parameterChangedTasks = new List<Task>();
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

        private void OnMessageReceived(object sender, ConnectionMessageEventArgs e)
        {
            if (sender is WsConnection connection && connection.UniqueId == WsChannelId)
            {
                if (e.Type == MessageType.Data && !e.IsControlMessage())
                {
                    ProcessWsMessage(e.Message);
                }
            }
        }

        #endregion

        #region Methods

        private void ProcessWsMessage(string json)
        {
            if (WsConnection.IsJson(json))
            {
                var parameterChangedTask = Task.Run(() =>
                {
                    var parameterSet = json.TryDeserializeObject<ParameterValueUpdateList>();

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
                        var parameterEntry = json.TryDeserializeObject<ParameterValueUpdate>();

                        if (parameterEntry != null)
                        {
                            OnParameterValueChanged(new ParameterValueChangedEventArgs(parameterEntry.NodeId,
                                                                                       parameterEntry.Index,
                                                                                       parameterEntry.SubIndex, parameterEntry.ParameterValue));
                        }
                    }
                });

                lock (this)
                {
                    _parameterChangedTasks.Add(parameterChangedTask);
                }
            }
            else
            {
                MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"unknown message '{json}' received");
            }
        }

        protected override async Task Execute()
        {
            const int executeIntervalWs = 1;
            const int reconnectIntervalWs = 1000;

            Status = WsChannelStatus.Starting;

            await ConnectToChannel();

            ConnectionManager.MessageReceived += OnMessageReceived;

            while (ShouldRun())
            {
                await ConnectionManager.Receive(WsChannelId, ShouldRun);

                if (ShouldRun())
                {
                    await ReconnectToWsChannel();

                    if (ConnectionManager.IsConnected(WsChannelId))
                    {
                        await Task.Delay(executeIntervalWs);
                    }
                    else
                    {
                        await Task.Delay(reconnectIntervalWs);
                    }   
                }
            }

            Task.WaitAll(_parameterChangedTasks.ToArray());

            Status = WsChannelStatus.Stopping;

            ConnectionManager.MessageReceived -= OnMessageReceived;

            await DisconnectFromWsChannel();

            Status = WsChannelStatus.Stopped;
        }

        #endregion
    }
}

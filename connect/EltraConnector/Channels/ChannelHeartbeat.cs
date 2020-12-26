using System.Diagnostics;
using System.Threading.Tasks;
using EltraCommon.Logger;
using System;
using EltraConnector.Controllers.Base;
using EltraConnector.Events;
using EltraCommon.Contracts.Channels;

namespace EltraConnector.Channels
{
    class ChannelHeartbeat : WsChannelThread
    {
        #region Private fields

        const string ChannelName = "SessionUpdate";

        private readonly ChannelControllerAdapter _channelControllerAdapter;
        private readonly uint _updateInterval;
        private readonly uint _timeout;
        private ChannelStatus _status = ChannelStatus.Offline;
                
        #endregion

        #region Constructors

        public ChannelHeartbeat(ChannelControllerAdapter channelControllerAdapter, string wsChannelName, string wsChannelId, uint updateInterval, uint timeout)
            : base(channelControllerAdapter.ConnectionManager,
                  wsChannelId, wsChannelName,
                  channelControllerAdapter.Channel.Id, channelControllerAdapter.User.Identity)
        {
            _updateInterval = updateInterval;
            _timeout = timeout;
            _channelControllerAdapter = channelControllerAdapter;
        }

        #endregion

        #region Properties

        public ChannelStatus ChannelStatus
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;

                    OnChannelStatusChanged();
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<AgentChannelStatusChangedEventArgs> ChannelStatusChanged;

        #endregion

        #region Event handling

        private void OnChannelStatusChanged()
        {
            ChannelStatusChanged?.Invoke(this, new AgentChannelStatusChangedEventArgs() { Status = _status, Id = ChannelId });
        }

        #endregion

        #region Methods

        protected override async Task Execute()
        {
            const int minWaitTime = 10;
            const int reconnectIntervalWs = 1000;
            const int executeIntervalWs = 1;

            uint updateIntervalInSec = _updateInterval;

            await ConnectToChannel();

            while (ShouldRun())
            {
                MsgLogger.Write($"{GetType().Name} - Execute", $"Updating session id = '{WsChannelId}'...");

                var updateResult = await _channelControllerAdapter.Update();

                ChannelStatus = updateResult ? ChannelStatus.Online : ChannelStatus.Offline;

                if (!updateResult)
                {
                    MsgLogger.WriteError($"{GetType().Name} - Execute", $"Update session '{WsChannelId}' failed!");
                }

                var waitWatch = new Stopwatch();

                waitWatch.Start();

                int channelStatusUpdateTimeout = (int)TimeSpan.FromSeconds(updateIntervalInSec).TotalMilliseconds;

                while (waitWatch.ElapsedMilliseconds < channelStatusUpdateTimeout && ShouldRun())
                {
                    await Task.Delay(minWaitTime);
                }

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

            await DisconnectFromWsChannel();

            ChannelStatus = ChannelStatus.Offline;

            MsgLogger.WriteLine($"Sync agent working thread finished successfully!");
        }

        protected override async Task<bool> ReconnectToWsChannel()
        {
            bool result = await base.ReconnectToWsChannel();

            if (result)
            {
                var updateResult = await _channelControllerAdapter.Update();

                ChannelStatus = updateResult ? ChannelStatus.Online : ChannelStatus.Offline;
            }

            return result;
        }

        #endregion
    }
}

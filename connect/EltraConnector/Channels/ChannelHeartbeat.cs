using System.Diagnostics;
using System.Threading.Tasks;
using EltraCommon.Logger;
using System;
using EltraConnector.Controllers.Base;
using EltraConnector.Events;
using EltraCommon.Contracts.Channels;
using EltraConnector.Transport.Ws;

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

        public ChannelHeartbeat(ChannelControllerAdapter channelControllerAdapter, uint updateInterval, uint timeout)
            : base(channelControllerAdapter.WsConnectionManager, 
                  channelControllerAdapter.Channel.Id, ChannelName,
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
            const int reconnectTimeout = 3;

            uint updateIntervalInSec = _updateInterval;

            await ConnectToWsChannel();

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

                int sessionUpdateTimeout;

                if (WsConnectionManager.IsConnected(WsChannelId))
                {
                    sessionUpdateTimeout = (int)TimeSpan.FromSeconds(updateIntervalInSec).TotalMilliseconds;
                }
                else
                {
                    sessionUpdateTimeout = (int)TimeSpan.FromSeconds(reconnectTimeout).TotalMilliseconds;
                }

                while (waitWatch.ElapsedMilliseconds < sessionUpdateTimeout && ShouldRun())
                {
                    await Task.Delay(minWaitTime);
                }

                if (ShouldRun())
                {
                    await ReconnectToWsChannel();
                }
            }

            ChannelStatus = ChannelStatus.Offline;

            await DisconnectFromWsChannel();

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

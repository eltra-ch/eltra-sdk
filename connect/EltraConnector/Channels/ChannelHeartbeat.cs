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

        private readonly ChannelControllerAdapter _channelControllerAdapter;
        private readonly uint _updateInterval;
        private ChannelStatus _status = ChannelStatus.Offline;
                
        #endregion

        #region Constructors

        public ChannelHeartbeat(ChannelControllerAdapter channelControllerAdapter, string wsChannelName, string wsChannelId, uint updateInterval, uint timeout)
            : base(channelControllerAdapter.ConnectionManager,
                  wsChannelId, wsChannelName,
                  channelControllerAdapter.Channel.Id, channelControllerAdapter.User.Identity)
        {
            _updateInterval = updateInterval;
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

        private async Task<bool> UpdateChannelStatus()
        {
            const int maxRetryCount = 1;

            bool result = await UpdateStatus(maxRetryCount);

            ChannelStatus = result ? ChannelStatus.Online : ChannelStatus.Offline;

            return result;
        }

        protected override async Task Execute()
        {
            const int minWaitTime = 10;
            const int reconnectIntervalWs = 1000;
            const int executeIntervalWs = 1;
            
            
            uint updateIntervalInSec = _updateInterval;

            try
            {
                await ConnectToChannel();

                while (ShouldRun())
                {
                    MsgLogger.Write($"{GetType().Name} - Execute", $"Updating channel id = '{WsChannelId}'...");

                    bool updateResult = await UpdateChannelStatus();

                    if (!updateResult)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Execute", $"Update channel '{WsChannelId}' failed!");
                    }
                    else
                    {
                        var waitWatch = new Stopwatch();

                        waitWatch.Start();

                        int channelStatusUpdateTimeout = (int)TimeSpan.FromSeconds(updateIntervalInSec).TotalMilliseconds;

                        while (waitWatch.ElapsedMilliseconds < channelStatusUpdateTimeout && ShouldRun())
                        {
                            await Task.Delay(minWaitTime);
                        }
                    }

                    if (ShouldRun())
                    {
                        if (!await ReconnectToWsChannel())
                        {
                            await Task.Delay(reconnectIntervalWs);
                        }
                        else
                        {
                            if (ConnectionManager.IsConnected(WsChannelId) && ChannelStatus == ChannelStatus.Online)
                            {
                                await Task.Delay(executeIntervalWs);
                            }
                            else
                            {
                                await Task.Delay(reconnectIntervalWs);
                            }
                        }
                    }
                }

                await DisconnectFromWsChannel();

                ChannelStatus = ChannelStatus.Offline;
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Execute", e);
            }

            MsgLogger.WriteLine($"{GetType().Name} - RunMaster", $"Sync agent working thread finished successfully!");
        }

        private async Task<bool> UpdateStatus(int maxRetryCount, int retryDelay = 1000)
        {
            bool result;
            int retryCount = 0;
            
            do
            {
                result = await _channelControllerAdapter.Update();

                if (!result)
                {
                    MsgLogger.WriteError($"{GetType().Name} - Execute", $"Update channel '{WsChannelId}' status failed, retry count {retryCount}/{maxRetryCount}!");

                    await Task.Delay(retryDelay);

                    retryCount++;
                }
            }
            while (!result && retryCount < maxRetryCount);

            return result;
        }

        private async Task<bool> ReconnectWithRetryCount(int maxRetryCount, int delay = 1000)
        {
            int retryCount = 0;
            bool result;

            do
            {
                result = await base.ReconnectToWsChannel();

                if (result)
                {
                    result = await _channelControllerAdapter.Update();
                }

                if(!result && ShouldRun())
                {
                    await Task.Delay(delay);
                }

                retryCount++;
            }
            while (!result && retryCount < maxRetryCount && ShouldRun());

            return result;
        }

        protected override async Task DisconnectFromWsChannel()
        {
            if (_channelControllerAdapter != null)
            {
                await _channelControllerAdapter.UnregisterChannel();
            }

            await base.DisconnectFromWsChannel();
        }

        protected override async Task<bool> ReconnectToWsChannel()
        {
            const int maxRetryCount = 3;

            bool result = await base.ReconnectToWsChannel();

            if (result)
            {
                result = await _channelControllerAdapter.Update();

                if(!result)
                {
                    result = await ReconnectWithRetryCount(maxRetryCount);

                    if (result)
                    {
                        MsgLogger.WriteFlow($"{GetType().Name} - ReconnectToWsChannel", "Update retried successfully");
                    }
                }

                ChannelStatus = result ? ChannelStatus.Online : ChannelStatus.Offline;

                if(!result)
                {
                    MsgLogger.WriteError($"{GetType().Name} - ReconnectToWsChannel", "Update failed, change status to offline");
                }
            }

            return result;
        }

        #endregion
    }
}

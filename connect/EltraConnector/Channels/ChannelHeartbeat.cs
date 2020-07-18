using System.Diagnostics;
using System.Threading.Tasks;
using EltraCommon.Logger;
using System;
using EltraCommon.Threads;
using EltraConnector.Controllers.Base;
using EltraConnector.Events;
using EltraCommon.Contracts.Channels;

namespace EltraConnector.Sessions
{
    class ChannelHeartbeat : EltraThread
    {
        #region Private fields

        private readonly ChannelControllerAdapter _sessionControllerAdapter;
        private readonly uint _updateInterval;
        private readonly uint _timeout;
        private ChannelStatus _status = ChannelStatus.Offline;
        private string _uuid;

        #endregion

        #region Constructors

        public ChannelHeartbeat(ChannelControllerAdapter sessionControllerAdapter, uint updateInterval, uint timeout)
        {
            _updateInterval = updateInterval;
            _timeout = timeout;
            _sessionControllerAdapter = sessionControllerAdapter;            
        }

        #endregion

        #region Properties

        public ChannelStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;

                    OnStatusChanged();
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<ChannelStatusChangedEventArgs> StatusChanged;

        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, new ChannelStatusChangedEventArgs() { Status = _status, Id = _uuid });
        }

        #endregion

        #region Methods

        protected override async Task Execute()
        {
            const int minWaitTime = 10;
            const int reconnectTimeout = 3;
                       
            var sessionUuid = _sessionControllerAdapter.Channel.Id;
            var wsConnectionManager = _sessionControllerAdapter.WsConnectionManager;
            string wsChannelName = "SessionUpdate";
            uint updateIntervalInSec = _updateInterval;

            if (wsConnectionManager.CanConnect(sessionUuid))
            {
                await wsConnectionManager.Connect(sessionUuid, wsChannelName);
            }

            _uuid = _sessionControllerAdapter.Channel.Id;

            while (ShouldRun())
            {
                MsgLogger.Write($"{GetType().Name} - Execute", $"Updating session id = '{sessionUuid}'...");

                var updateResult = await _sessionControllerAdapter.Update();

                Status = updateResult ? ChannelStatus.Online : ChannelStatus.Offline;

                if (!updateResult)
                {
                    MsgLogger.WriteError($"{GetType().Name} - Execute", $"Update session '{sessionUuid}' failed!");

                    if(wsConnectionManager.IsConnected(sessionUuid))
                    {
                        await wsConnectionManager.Disconnect(sessionUuid);
                    }
                }
                
                var waitWatch = new Stopwatch();
                
                waitWatch.Start();

                int sessionUpdateTimeout;
                
                if (wsConnectionManager.IsConnected(sessionUuid))
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

                if (!wsConnectionManager.IsConnected(sessionUuid) && wsConnectionManager.CanConnect(sessionUuid))
                {
                    if (await wsConnectionManager.Connect(sessionUuid, wsChannelName))
                    {
                        updateResult = await _sessionControllerAdapter.Update();

                        Status = updateResult ? ChannelStatus.Online : ChannelStatus.Offline;
                    }
                }
            }

            if(wsConnectionManager.IsConnected(sessionUuid))
            {
                await wsConnectionManager.Disconnect(sessionUuid);
            }

            MsgLogger.WriteLine($"Sync agent working thread finished successfully!");
        }

        #endregion
    }
}

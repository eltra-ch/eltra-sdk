using System.Diagnostics;
using System.Threading.Tasks;
using EltraCommon.Logger;
using System;
using EltraCommon.Threads;
using EltraConnector.Controllers.Base;
using EltraConnector.Events;
using EltraCommon.Contracts.Channels;
using EltraConnector.Transport.Ws;

namespace EltraConnector.Sessions
{
    class ChannelHeartbeat : EltraThread
    {
        #region Private fields

        private readonly ChannelControllerAdapter _channelControllerAdapter;
        private readonly uint _updateInterval;
        private readonly uint _timeout;
        private ChannelStatus _status = ChannelStatus.Offline;
        private string _channelId;
        private WsConnectionManager _wsConnectionManager;
      
        #endregion

        #region Constructors

        public ChannelHeartbeat(ChannelControllerAdapter channelControllerAdapter, uint updateInterval, uint timeout)
        {
            _updateInterval = updateInterval;
            _timeout = timeout;
            _channelControllerAdapter = channelControllerAdapter;            
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

        public event EventHandler<AgentChannelStatusChangedEventArgs> StatusChanged;
        public event EventHandler<SignInRequestEventArgs> SignInRequested;

        #endregion

        #region Event handling

        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, new AgentChannelStatusChangedEventArgs() { Status = _status, Id = _channelId });
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
            RequestStop();

            Task.Run(async () => {
                if (_wsConnectionManager.IsConnected(_channelId))
                {
                    await _wsConnectionManager.Disconnect(_channelId);
                }
            });

            return base.Stop();
        }

        protected override async Task Execute()
        {
            const int minWaitTime = 10;
            const int reconnectTimeout = 3;

            _channelId = _channelControllerAdapter.Channel.Id;
            _wsConnectionManager = _channelControllerAdapter.WsConnectionManager;
            
            string wsChannelName = "SessionUpdate";
            uint updateIntervalInSec = _updateInterval;

            if (_wsConnectionManager.CanConnect(_channelId))
            {
                if(await _wsConnectionManager.Connect(_channelId, wsChannelName))
                {
                    await SendSessionIdentyfication(_channelId);
                }
            }

            while (ShouldRun())
            {
                MsgLogger.Write($"{GetType().Name} - Execute", $"Updating session id = '{_channelId}'...");

                var updateResult = await _channelControllerAdapter.Update();

                Status = updateResult ? ChannelStatus.Online : ChannelStatus.Offline;

                if (!updateResult)
                {
                    MsgLogger.WriteError($"{GetType().Name} - Execute", $"Update session '{_channelId}' failed!");
                }
                
                var waitWatch = new Stopwatch();
                
                waitWatch.Start();

                int sessionUpdateTimeout;
                
                if (_wsConnectionManager.IsConnected(_channelId))
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

                if (!_wsConnectionManager.IsConnected(_channelId) && _wsConnectionManager.CanConnect(_channelId))
                {
                    if (OnSignInRequested())
                    {
                        if (await _wsConnectionManager.Connect(_channelId, wsChannelName))
                        {
                            await SendSessionIdentyfication(_channelId);

                            updateResult = await _channelControllerAdapter.Update();

                            Status = updateResult ? ChannelStatus.Online : ChannelStatus.Offline;
                        }
                    }
                }
            }

            if(_wsConnectionManager.IsConnected(_channelId))
            {
                await _wsConnectionManager.Disconnect(_channelId);
            }

            MsgLogger.WriteLine($"Sync agent working thread finished successfully!");
        }

        private async Task SendSessionIdentyfication(string sessionUuid)
        {
            if (_wsConnectionManager != null && _wsConnectionManager.IsConnected(sessionUuid))
            {
                var sessionIdent = new ChannelIdentification() { Id = _channelControllerAdapter.ChannelId };

                await _wsConnectionManager.Send(sessionUuid, _channelControllerAdapter.User.Identity, sessionIdent);
            }
        }

        #endregion
    }
}

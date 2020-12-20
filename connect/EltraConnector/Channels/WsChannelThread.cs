using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraCommon.Threads;
using EltraConnector.Channels.Events;
using EltraConnector.Events;
using EltraConnector.Transport.Ws.Interfaces;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EltraConnector.Channels
{
    class WsChannelThread : EltraThread
    {
        #region Private fields
                
        private readonly string _wsChannelName;
        private readonly string _wsChannelId;
        private readonly IConnectionManager _connectionManager;
        
        private readonly int _nodeId;
        private readonly string _channelId;
        private readonly UserIdentity _identity;

        private WsChannelStatus _status;
        private int _startupTimeout = 30000;
        private int _shutdownTimeout = 30000;

        #endregion

        #region Constructors

        public WsChannelThread(IConnectionManager connectionManager, string wsChannelId, string wsChannelName, string channelId, int nodeId, UserIdentity identity)
        {
            _connectionManager = connectionManager;
            _wsChannelId = wsChannelId;
            _wsChannelName = wsChannelName;
            _channelId = channelId;
            _nodeId = nodeId;
            _identity = identity;
        }

        public WsChannelThread(IConnectionManager connectionManager, string wsChannelId, string wsChannelName, string channelId, UserIdentity identity)
        {
            _connectionManager = connectionManager;
            _wsChannelId = wsChannelId;
            _wsChannelName = wsChannelName;
            _channelId = channelId;
            _identity = identity;
        }

        #endregion

        #region Events

        public event EventHandler<WsChannelStatusEventArgs> StatusChanged;
        
        public event EventHandler<SignInRequestEventArgs> SignInRequested;

        #endregion

        #region Events handling

        protected virtual void OnStatusChanged(WsChannelStatusEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        private bool OnSignInRequested()
        {
            var args = new SignInRequestEventArgs();

            SignInRequested?.Invoke(this, args);

            return args.SignInResult;
        }

        #endregion

        #region Properties

        public WsChannelStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;

                    OnStatusChanged(new WsChannelStatusEventArgs() { Status = Status });
                }
            }
        }

        protected IConnectionManager ConnectionManager => _connectionManager;

        protected string WsChannelId => _wsChannelId;

        protected string WsChannelName => _wsChannelName;

        public string ChannelId => _channelId;

        public int NodeId => _nodeId;

        #endregion

        #region Methods

        public override void Start()
        {
            base.Start();

            if (Status != WsChannelStatus.Started)
            {
                const int minWaitTime = 10;

                Status = WsChannelStatus.Undefined;

                var stopWatch = new Stopwatch();
                bool result = false;

                stopWatch.Start();

                StatusChanged += (o, e) =>
                {
                    if (e.Status == WsChannelStatus.Started)
                    {
                        result = true;
                    }
                };

                while (!result && stopWatch.ElapsedMilliseconds < _startupTimeout && 
                    Status != WsChannelStatus.Stopped)
                {
                    Thread.Sleep(minWaitTime);
                }
            }
        }

        public override bool Stop()
        {
            bool result = false;

            RequestStop();

            if (Status != WsChannelStatus.Stopped)
            {
                const int minWaitTime = 10;

                var stopWatch = new Stopwatch();
                
                stopWatch.Start();

                StatusChanged += (o, e) =>
                {
                    if (e.Status == WsChannelStatus.Stopped)
                    {
                        result = true;
                    }
                };

                Task.Run(async () =>
                {
                    await DisconnectFromWsChannel();
                });

                while (!result && stopWatch.ElapsedMilliseconds < _shutdownTimeout && Status != WsChannelStatus.Stopped)
                {
                    Thread.Sleep(minWaitTime);
                }
            }

            base.Stop();

            return result;
        }

        protected async Task<bool> ConnectToChannel()
        {
            bool result = false;

            if (ConnectionManager.CanConnect(WsChannelId))
            {
                if (OnSignInRequested())
                {
                    Status = WsChannelStatus.Starting;

                    if (await ConnectionManager.Connect(WsChannelId, WsChannelName))
                    {
                        result = await SendChannelIdentyficationRequest();
                    
                        if(result)
                        {
                            Status = WsChannelStatus.Started;
                        }
                    }
                }
            }
            else if(ConnectionManager.IsConnected(WsChannelId))
            {
                Status = WsChannelStatus.Started;
                result = true;
            }

            return result;
        }

        protected async Task DisconnectFromWsChannel()
        {
            if (ConnectionManager.IsConnected(WsChannelId))
            {
                await ConnectionManager.Disconnect(WsChannelId);
            }
            else
            {
                ConnectionManager.Remove(WsChannelId);
            }

            Status = WsChannelStatus.Stopped;
        }

        private async Task<bool> SendChannelIdentyficationRequest()
        {
            bool result = false;

            if (ConnectionManager.IsConnected(WsChannelId))
            {
                var request = new ChannelIdentification() { Id = _channelId, NodeId = _nodeId };

                result = await ConnectionManager.Send(WsChannelId, _identity, request);

                if(result)
                {
                    Status = WsChannelStatus.Started;
                }
            }
            else
            {
                Status = WsChannelStatus.Stopped;
            }

            return result;
        }

        protected virtual async Task<bool> ReconnectToWsChannel()
        {
            bool result = false;

            if (ConnectionManager.IsConnected(WsChannelId))
            {
                Status = WsChannelStatus.Started;

                result = true;
            }
            else if (ConnectionManager.CanConnect(WsChannelId))
            {
                if (OnSignInRequested())
                {
                    Status = WsChannelStatus.Starting;

                    if (await ConnectionManager.Connect(WsChannelId, WsChannelName))
                    {
                        result = await SendChannelIdentyficationRequest();
                    
                        if(result)
                        {
                            Status = WsChannelStatus.Started;
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}

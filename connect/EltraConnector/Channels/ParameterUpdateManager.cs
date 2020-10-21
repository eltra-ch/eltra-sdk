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
using EltraConnector.Channels.Events;
using System.Diagnostics;
using System.Threading;

namespace EltraConnector.UserAgent
{
    class ParameterUpdateManager : EltraThread
    {
        #region Private fields

        private readonly ChannelControllerAdapter _channelAdapter;        
        private readonly WsConnectionManager _wsConnectionManager;
        private string _wsChannelName;
        private string _wsChannelId;
        private int _nodeId;
        private WsChannelStatus _status;

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
            _nodeId = nodeId;
            _wsConnectionManager = channelAdapter.WsConnectionManager;

            _channelAdapter = channelAdapter;
            _wsChannelId = _channelAdapter.ChannelId + $"_ParameterUpdate_{nodeId}";
            _wsChannelName = "ParameterUpdate";
        }

        #endregion

        #region Events

        public event EventHandler<ParameterValueChangedEventArgs> ParameterValueChanged;

        public event EventHandler<SignInRequestEventArgs> SignInRequested;

        public event EventHandler<WsChannelStatusEventArgs> StatusChanged;

        #endregion

        #region Events handling

        protected virtual void OnParameterValueChanged(ParameterValueChangedEventArgs e)
        {
            ParameterValueChanged?.Invoke(this, e);
        }

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

        #endregion

        #region Methods

        public override void Start()
        {
            base.Start();

            const int minWaitTime = 1;
            const long maxWaitTime = 60000;

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

            while (!result && stopWatch.ElapsedMilliseconds < maxWaitTime)
            {
                Thread.Sleep(minWaitTime);
            }
        }

        public override bool Stop()
        {
            Status = WsChannelStatus.Stopping;

            RequestStop();

            Task.Run(async ()=> await CloseWsChannel(_wsChannelId));

            return base.Stop();
        }

        private async Task<bool> SendSessionIdentyfication(string commandExecUuid)
        {
            bool result = false;

            if (_wsConnectionManager.IsConnected(commandExecUuid))
            {
                var sessionIdent = new ChannelIdentification() { Id = _channelAdapter.ChannelId, NodeId = _nodeId };

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

            Status = WsChannelStatus.Starting;

            var parameterChangedTasks = new List<Task>();

            if(await CreateWsChannel(_wsChannelId, _wsChannelName))
            {
                Status = WsChannelStatus.Started;
            }
            
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
                    Status = WsChannelStatus.Starting;

                    if(await CreateWsChannel(_wsChannelId, _wsChannelName))
                    {
                        Status = WsChannelStatus.Started;
                    }

                    await Task.Delay(executeIntervalWs);
                }
            }

            foreach(var parameterChangedTask in parameterChangedTasks)
            {
                parameterChangedTask.Wait();
            }

            await CloseWsChannel(_wsChannelId);

            Status = WsChannelStatus.Stopped;
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

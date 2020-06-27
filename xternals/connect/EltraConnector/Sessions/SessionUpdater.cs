using System.Diagnostics;
using System.Threading.Tasks;
using EltraCommon.Logger;
using System;
using EltraCommon.Threads;
using EltraConnector.Controllers.Base;

namespace EltraConnector.Sessions
{
    class SessionUpdater : EltraThread
    {
        #region Private fields

        private readonly SessionControllerAdapter _sessionControllerAdapter;
        private readonly uint _updateInterval;
        private readonly uint _timeout;

        #endregion

        #region Constructors

        public SessionUpdater(SessionControllerAdapter sessionControllerAdapter, uint updateInterval, uint timeout)
        {
            _updateInterval = updateInterval;
            _timeout = timeout;
            _sessionControllerAdapter = sessionControllerAdapter;            
        }

        #endregion

        #region Methods

        protected override async Task Execute()
        {
            const int minWaitTime = 10;
            const int reconnectTimeout = 3;
                       
            var sessionUuid = _sessionControllerAdapter.Session.Uuid;
            var wsConnectionManager = _sessionControllerAdapter.WsConnectionManager;
            string wsChannelName = "SessionUpdate";
            uint updateIntervalInSec = _updateInterval;

            if (wsConnectionManager.CanConnect(sessionUuid))
            {
                await wsConnectionManager.Connect(sessionUuid, wsChannelName);
            }

            while (ShouldRun())
            {
                MsgLogger.Write($"{GetType().Name} - Execute", $"Updating session id = '{sessionUuid}'...");

                var updateResult = await _sessionControllerAdapter.Update();

                if (!updateResult)
                {
                    MsgLogger.WriteError($"{GetType().Name} - Execute", $"Update session '{sessionUuid}' failed!");

                    /*if(wsConnectionManager.IsConnected(sessionUuid))
                    {
                        await wsConnectionManager.Disconnect(sessionUuid);
                    }*/
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
                        await _sessionControllerAdapter.Update();
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

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EltraCommon.Logger;

namespace EltraCommon.Threads
{
    public class EltraThread
    {
        #region Private fields

        private readonly ManualResetEvent _stopRequestEvent;
        private readonly ManualResetEvent _running;
        private readonly Thread _workingThread;
        private object _lock = new object();

        #endregion

        #region Constructors

        public EltraThread()
        {
            _stopRequestEvent = new ManualResetEvent(false);
            _running = new ManualResetEvent(false);
            _workingThread = new Thread(Run);
        }

        #endregion

        #region Properties

        public bool IsRunning => _running.WaitOne(0);

        #endregion

        #region Methods
        
        protected void SetRunning()
        {
            lock (_lock)
            {
                _running.Set();
            }
        }

        protected void SetStopped()
        {
            _running.Reset();
        }

        protected void RequestStop()
        {
            _stopRequestEvent?.Set();
        }

        protected bool ShouldRun()
        {
            return !_stopRequestEvent.WaitOne(0);
        }
        
        public virtual bool Stop()
        {
            const int minWaitTimeMs = 10;
            
            bool result = false;
            int maxWaitTime = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;

            if (IsRunning)
            {
                lock (_lock)
                {
                    RequestStop();

                    var timeout = new Stopwatch();

                    timeout.Start();

                    while (_running.WaitOne(minWaitTimeMs) && timeout.ElapsedMilliseconds < maxWaitTime)
                    {
                        Thread.Sleep(minWaitTimeMs);
                    }

                    if (timeout.ElapsedMilliseconds > maxWaitTime)
                    {
                        MsgLogger.WriteError($"{GetType().Name}::Stop", "stop thread timeout");
                    }
                    else
                    {
                        result = true;
                    }
                }                
            }

            return result;
        }

        private async void Run()
        {
            SetRunning();

            const int minDelay = 10;

            while (ShouldRun())
            {
                try
                {
                    await Execute();
                }
                catch (Exception e)
                {
                    MsgLogger.Exception("EltraThread - Run", e);
                }

                await Task.Delay(minDelay);
            }

            SetStopped();
        }

        protected virtual Task Execute()
        {
            return Task.Run(()=>{ });
        }

        public virtual void Start()
        {
            int minWaitTime = 1;

            if (!IsRunning)
            {
                _workingThread.Start();

                while (!IsRunning)
                {
                    Thread.Sleep(minWaitTime);
                }
            }            
        }

        public Task StartAsync()
        {
            Task result = null;

            if (!IsRunning)
            {
                result = Task.Run(() =>
                {
                    Run();
                });
            }

            return result;
        }

        public void Restart()
        {
            Stop();

            Start();
        }

        #endregion
    }
}

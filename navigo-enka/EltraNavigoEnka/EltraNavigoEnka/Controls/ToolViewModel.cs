using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EltraCommon.Logger;
using Xamarin.Forms;

namespace EltraNavigo.Controls
{
    public class ToolViewModel : ToolViewBaseModel
    {
        #region Private fields

        private readonly ManualResetEvent _stopRequestEvent;
        private readonly ManualResetEvent _stopped;
        private readonly ManualResetEvent _running;
        
        #endregion

        #region Constructors

        public ToolViewModel()
        {
            const int defaultUpdateInterval = 1000;

            _stopRequestEvent = new ManualResetEvent(false);
            _stopped = new ManualResetEvent(false);
            _running = new ManualResetEvent(false);

            UpdateInterval = defaultUpdateInterval;

            IsSupported = true;
            Persistenced = true;
        }

        public ToolViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            const int defaultUpdateInterval = 1000;

            _stopRequestEvent = new ManualResetEvent(false);
            _stopped = new ManualResetEvent(false);
            _running = new ManualResetEvent(false);

            UpdateInterval = defaultUpdateInterval;

            IsSupported = true;
            Persistenced = true;
        }

        #endregion

        #region Properties
        
        public bool UpdateViewModels { get; set; }

        public int UpdateInterval { get; set; }

        public ImageSource Image { get; set; } 

        public string Uuid { get; set; }

        public bool Persistenced { get; set; }

        #endregion

        #region Events

        public event EventHandler DeviceInitialized;

        #endregion

        #region Events handling

        private void OnDeviceInitialized()
        {
            DeviceInitialized?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        protected virtual void OnAgentChanged()
        {
            Init();
        }

        protected virtual void OnDeviceChanged()
        {
            Init();
        }

        public virtual void Clear()
        {
        }

        public void Init()
        {
            if (UpdateViewModels)
            {
                Task.Run(() => { Run(); });
            }
        }
        
        private async void Run()
        {
            const int updateSessionInterval = 200;

            _running.Set();

            while (ShouldRun())
            {
                try
                {
                    await UpdateViewModelsTree();
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Run", e);
                }
                
                var watch = new Stopwatch();

                watch.Start();

                while (watch.ElapsedMilliseconds < UpdateInterval && ShouldRun())
                {
                    await Task.Delay(updateSessionInterval);
                }
            }

            _stopped.Set();
        }
        
        private bool ShouldRun()
        {
            return !_stopRequestEvent.WaitOne(0);
        }

        private bool IsRunning()
        {
            return _running.WaitOne(0);
        }

        public void Stop()
        {
            if (_running.WaitOne(0))
            {
                _stopRequestEvent.Set();

                _stopped.WaitOne();

                _running.Reset();
                _stopRequestEvent.Reset();
            }
        }

        public virtual void ResetAgent()
        {
            foreach (var child in SafeChildrenArray)
            {
                if(child is ToolViewModel toolViewModel)
                {
                    toolViewModel.ResetAgent();
                }
            }
        }

        #endregion
    }
}

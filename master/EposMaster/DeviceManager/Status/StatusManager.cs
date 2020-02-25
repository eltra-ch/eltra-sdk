using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EltraCloudContracts.Contracts.Devices;
using EposMaster.DeviceManager.Device;
using EposMaster.DeviceManager.VcsWrapper;

namespace EposMaster.DeviceManager.Status
{
    class StatusManager
    {
        #region Private fields

        private readonly EposDevice _device;
        private readonly ManualResetEvent _stopRequestEvent;
        private readonly ManualResetEvent _running;

        #endregion

        #region Constructors

        public StatusManager(EposDevice device)
        {
            _stopRequestEvent = new ManualResetEvent(false);
            _running = new ManualResetEvent(false);

            _device = device;
        }

        public bool IsRunning
        {
            get => _running.WaitOne(0);
        }

        #endregion

        #region Methods
       
        public void Run()
        {
            const int waitTimeout = 1000;
            const int minTimeout = 10;

            _running.Set();

            while (ShouldRun())
            {
                StatusUpdate();

                var delay = new Stopwatch();

                delay.Start();

                while (delay.ElapsedMilliseconds < waitTimeout && ShouldRun())
                {
                    Thread.Sleep(minTimeout);
                }
            }

            _running.Reset();
        }

        public bool CheckState()
        {
            bool result = false;
            var states = EStates.StQuickStop;
            var communication = _device?.Communication as Epos4DeviceCommunication;

            if (communication != null)
            {
                result = communication.GetState(ref states);
            }

            return result;
        }

        private async void StatusUpdate()
        {
            const int maxTryCount = 3;
            const int waitTimeout = 1000;
            const int minTimeout = 10;
            
            int tryCount = 0;
            
            while (!CheckState() && tryCount <= maxTryCount && ShouldRun())
            {
                tryCount++;

                var delay = new Stopwatch();
                delay.Start();

                while (delay.ElapsedMilliseconds < waitTimeout && ShouldRun())
                {
                    Thread.Sleep(minTimeout);
                }
            }

            if (tryCount >= maxTryCount && (_device.Status == DeviceStatus.Connected || _device.Status == DeviceStatus.Registered))
            {
                await Task.Run(() =>_device.Disconnect());
            }
            
        }

        private bool ShouldRun()
        {
            return !_stopRequestEvent.WaitOne(0);
        }

        public void Stop()
        {
            if (_running.WaitOne(0))
            {
                _stopRequestEvent.Set();

                while (_running.WaitOne(10));
            }
        }
        
        #endregion
    }
}

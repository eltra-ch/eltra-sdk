using EltraCloudContracts.Contracts.Devices;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;
using System;
using System.Threading.Tasks;

namespace EltraMaster.Device
{
    public class MasterDeviceManager : IDisposable
    {
        #region Private fields

        private readonly SyncCloudAgent _cloudAgent;
        private readonly MasterDevice _device;

        #endregion

        #region Constructors

        public MasterDeviceManager(SyncCloudAgent cloudAgent, MasterDevice device)
        {
            _device = device;
            
            _cloudAgent = cloudAgent;

            if (_device != null)
            {
                _device.CloudAgent = cloudAgent;
            }
        }

        #endregion

        #region Methods

        protected virtual async void Dispose(bool finalize)
        {
            if (finalize)
            {
                await Stop();
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public async Task Stop()
        {
            _device.Disconnect();

            MsgLogger.WriteLine($"Disconnected: device = {_device.Name} - Unregister");

            await _cloudAgent.UnregisterDevice(_device);

            _device.Status = DeviceStatus.Unregistered;
        }

        public async Task Run()
        {
            if (_device != null)
            {
                MsgLogger.WriteLine($"Connected: device='{_device.Name}', serial number=0x{_device.Identification.SerialNumber:X}");

                _device.Status = DeviceStatus.Ready;

                await _cloudAgent.RegisterDevice(_device);
            }
        }

        #endregion
    }
}

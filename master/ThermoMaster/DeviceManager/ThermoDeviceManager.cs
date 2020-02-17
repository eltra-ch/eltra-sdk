using System;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;

using EltraCloudContracts.Contracts.Devices;
using ThermoMaster.DeviceManager.Device;
using ThermoMaster.Settings;

namespace ThermoMaster.DeviceManager
{
    class ThermoDeviceManager : IDisposable
    {
        #region Private fields
        
        private readonly SyncCloudAgent _cloudAgent;
        private readonly MasterSettings _settings;
        private readonly ThermoDeviceBase _device;

        #endregion

        #region Constructors

        public ThermoDeviceManager(MasterSettings settings, SyncCloudAgent cloudAgent)
        {
            _settings = settings;
            _cloudAgent = cloudAgent;

            _device = new ThermoDevice(settings);

            _device.CloudAgent = _cloudAgent;

            _device.Pins = _settings.Device.Pins.ToArray();
        }

        #endregion

        #region Properties

        ulong SerialNumber
        {
            get
            {
                ulong result = 0;

                if (_settings!=null && _settings.Device!=null)
                {
                    result = _settings.Device.SerialNumber;
                }

                return result;
            }            
        }

        #endregion

        #region Methods

        public async void Dispose()
        {
            await Stop();            
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
                _device.Identification.SerialNumber = SerialNumber;
                               
                MsgLogger.WriteLine($"Connected: device='{_device.Name}', serial number=0x{_device.Identification.SerialNumber:X}");

                _device.Status = DeviceStatus.Ready;

                await _cloudAgent.RegisterDevice(_device);
            }
        }

        #endregion
    }
}

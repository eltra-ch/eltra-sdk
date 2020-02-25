using System;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;

using EltraCloudContracts.Contracts.Devices;
using EposMaster.DeviceManager.Device;
using EposMaster.DeviceManager.Events;
using EposMaster.DeviceManager.Scanner;
using EposMaster.Settings;
using EltraMaster.Device;

namespace EposMaster.DeviceManager
{
    class EposDeviceManager : MasterDeviceManager
    {
        #region Private fields
        
        private readonly DeviceScanner _deviceScanner;

        #endregion

        #region Constructors

        public EposDeviceManager(MasterSettings settings)
        {
            _deviceScanner = new DeviceScanner(settings);

            RegisterEvents();
        }

        public EposDeviceManager(MasterSettings settings, SyncCloudAgent cloudAgent)
            : base(cloudAgent)
        {
            _deviceScanner = new DeviceScanner(settings);
            
            RegisterEvents();
        }

        #endregion

        #region Events handling

        private async void OnDeviceDetected(object sender, ScannerDeviceStatusChangedEventArgs e)
        {
            var device = e.Device;
            
            if (device is EposDevice eposDevice)
            {
                AddDevice(device);

                RegisterDeviceEvents(eposDevice);

                MsgLogger.WriteDebug($"{GetType().Name} - method", $"try to connect to device {eposDevice.Name}");

                if (!await ConnectDevice(eposDevice))
                {
                    MsgLogger.WriteError($"{GetType().Name} - OnDeviceDetected", $"connect to device {eposDevice.Name} failed!");

                    UnregisterDeviceEvents(eposDevice);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - method", $"connect to device {eposDevice.Name} successful");
                }
            }
            else
            {
                MsgLogger.WriteLine($"device: {device.Name} not supported!");
            }
        }

        #endregion

        #region Methods

        protected override async void Dispose(bool finalize)
        {
            DisconnectAllDevices();

            await _deviceScanner.Stop();

            base.Dispose(finalize);
        }
                
        private void RegisterEvents()
        {
            _deviceScanner.DeviceDetected += OnDeviceDetected;
        }
        
        private void RegisterDeviceEvents(EposDevice device)
        {
            device.StatusChanged += OnDeviceStatusChanged;
        }

        private void UnregisterDeviceEvents(EposDevice device)
        {
            device.StatusChanged -= OnDeviceStatusChanged;
        }

        private async void OnDeviceStatusChanged(object sender, EventArgs e)
        {
            if (sender is EposDevice device)
            {
                switch (device.Status)
                {
                    case DeviceStatus.Connected:
                    {
                        MsgLogger.WriteLine($"Connected: device='{device.Name}', serial number=0x{device.Identification.SerialNumber:X}");

                        await CloudAgent.RegisterDevice(device);

                    } break;
                    case DeviceStatus.Disconnected:
                    {
                        MsgLogger.WriteLine($"Disconnected: device = {device.Name} - Unregister");

                        await CloudAgent.UnregisterDevice(device);

                        device.Status = DeviceStatus.Unregistered;
                    } break;
                }
            }
        }
        
        public void ScanDevices()
        {
            _deviceScanner.Scan();
        }

        private async Task<bool> ConnectDevice(EposDevice device)
        {
            bool result = false;

            if (CloudAgent != null)
            {
                device.CloudAgent = CloudAgent;

                result = await device.Connect();
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - ConnectDevice", $"connection to {device.Name} failed, agent not ready!");
            }

            return result;
        }

        private void DisconnectAllDevices()
        {
            _deviceScanner.DisconnectAll();
        }
        
        #endregion
    }
}

using System;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;

using EltraCommon.Contracts.Devices;
using EposMaster.DeviceManager.Device;
using EposMaster.DeviceManager.Events;
using EposMaster.DeviceManager.Scanner;
using EposMaster.Settings;
using EltraConnector.Master.Device;

namespace EposMaster.DeviceManager
{
    public class EposDeviceManager : MasterDeviceManager
    {
        #region Private fields
        
        private readonly DeviceScanner _deviceScanner;

        #endregion

        #region Constructors

        public EposDeviceManager(MasterSettings settings)
        {
            VcsWrapper.Device.Init();

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

                MsgLogger.WriteDebug($"{GetType().Name} - method", $"try to connect to device {eposDevice.Family}");

                if (!await ConnectDevice(eposDevice))
                {
                    MsgLogger.WriteError($"{GetType().Name} - OnDeviceDetected", $"connect to device {eposDevice.Family} failed!");

                    UnregisterDeviceEvents(eposDevice);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - method", $"connect to device {eposDevice.Family} successful");
                }
            }
            else
            {
                MsgLogger.WriteLine($"device: {device.Family} not supported!");
            }
        }

        #endregion

        #region Methods

        protected override async void Dispose(bool finalize)
        {
            DisconnectAllDevices();

            await _deviceScanner.Stop();

            VcsWrapper.Device.Cleanup();

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

        protected override void OnCloudAgentChanged()
        {
            base.OnCloudAgentChanged();

            
        }

        private async void OnDeviceStatusChanged(object sender, EventArgs e)
        {
            if (sender is EposDevice device)
            {
                switch (device.Status)
                {
                    case DeviceStatus.Connected:
                    {
                        MsgLogger.WriteLine($"Connected: device='{device.Family}', serial number=0x{device.Identification.SerialNumber:X}");

                        if(string.IsNullOrEmpty(device.Name))
                        {
                            device.Name = device.Identification.Name;
                        }

                        await CloudAgent.RegisterDevice(device);

                    } break;
                    case DeviceStatus.Disconnected:
                    {
                        MsgLogger.WriteLine($"Disconnected: device = {device.Family} - Unregister");

                        await CloudAgent.UnregisterDevice(device);

                        device.Status = DeviceStatus.Unregistered;
                    } break;
                }
            }
        }
        
        public override Task Run()
        {
            var task = Task.Run(() =>
            {
                _deviceScanner.Scan();
            });

            return task;
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
                MsgLogger.WriteError($"{GetType().Name} - ConnectDevice", $"connection to {device.Family} failed, agent not ready!");
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

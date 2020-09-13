using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraCommon.Contracts.Devices;
using EposMaster.DeviceManager.Device;
using EposMaster.DeviceManager.Device.Factory;
using EposMaster.DeviceManager.Events;
using EposMaster.Settings;

namespace EposMaster.DeviceManager.Scanner
{
    class DeviceScanner
    {
        #region Private fields

        private readonly CancellationTokenSource _cancelationTokenSource;
        private readonly List<MotionControllerDevice> _devices;
        private List<Task> _deviceScanTasks;
        private readonly MasterSettings _settings;

        #endregion

        #region Constructors

        public DeviceScanner(MasterSettings settings)
        {
            _settings = settings;
            _devices = new List<MotionControllerDevice>();
            _cancelationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Properties

        private MotionControllerDevice[] SafeDeviceArray
        {
            get
            {
                MotionControllerDevice[] result;

                lock (this)
                {
                    result = _devices.ToArray();
                }

                return result;
            }
        }

        #endregion

        #region Events

        public event EventHandler<ScannerDeviceStatusChangedEventArgs> DeviceDetected;

        #endregion

        #region Events handling

        protected virtual void OnDeviceDetected(ScannerDeviceStatusChangedEventArgs e)
        {
            DeviceDetected?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public async Task Stop()
        {
            _cancelationTokenSource.Cancel();

            if (_deviceScanTasks != null)
            {
                await Task.WhenAll(_deviceScanTasks);
            }
        }

        public void Scan()
        {
            var supportedDeviceNames = _settings.Scanning.DeviceNames;

            _deviceScanTasks = new List<Task>();

            foreach (var deviceName in supportedDeviceNames)
            {
                var scanTask = UpdateAvailableDevicesListAsync(deviceName);

                _deviceScanTasks.Add(scanTask);
            }
        }

        private Task UpdateAvailableDevicesListAsync(string deviceName)
        {
            MsgLogger.WriteLine($"Update device: {deviceName}");

            return Task.Run(() => UpdateAvailableDevicesList(_cancelationTokenSource.Token, deviceName));
        }

        private MotionControllerDevice FindScannerDevice(MotionControllerDevice[] motionControllerDevices, MotionControllerDevice device)
        {
            MotionControllerDevice result = null;

            foreach (var scannerDevice in motionControllerDevices)
            {
                if (scannerDevice.Equals(device))
                {
                    result = scannerDevice;
                    break;
                }
            }

            return result;
        }

        private async Task<List<MotionControllerDevice>> GetDevices(string familyName)
        {
            var result = new List<MotionControllerDevice>();
            var protocolStackNames = GetProtocolStackNames(familyName);
            var scanningSettings = _settings.Scanning;

            foreach (var protocolStackName in protocolStackNames)
            {
                if (scanningSettings.Skip.SkipProtocolStack(protocolStackName))
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - method", $"skip protocol stack name: {protocolStackName}");
                    continue;
                }

                var interfaceNames = GetProtocolInterfaceNames(familyName, protocolStackName);

                foreach (var interfaceName in interfaceNames)
                {
                    if (scanningSettings.Skip.SkipInterface(interfaceName))
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - method", $"skip interface name: {interfaceName}");
                        continue;
                    }

                    var portNames = await GetPortNamesAsync(familyName, protocolStackName, interfaceName);

                    foreach (var portName in portNames)
                    {
                        var device = new MotionControllerDevice(familyName, ""/*TODO*/, 1)
                        {
                            ProtocolStackName = protocolStackName,
                            InterfaceName = interfaceName,
                            Family = familyName,
                            PortName = portName
                        };

                        if (FindScannerDevice(result.ToArray(), device) == null)
                        {
                            result.Add(device);
                        }
                    }
                }
            }

            return result;
        }
        
        private async Task UpdateAvailableDevicesList(CancellationToken token, string deviceName)
        {
            const int deviceUpdateWaitTime = 1000;

            do
            {
                var devices = await GetDevices(deviceName);

                foreach (var device in devices)
                {
                    var existingDevice = FindScannerDevice(SafeDeviceArray, device);

                    if (existingDevice == null || existingDevice.Status == DeviceStatus.Undefined || 
                                                  existingDevice.Status == DeviceStatus.Disconnected ||
                                                  existingDevice.Status == DeviceStatus.Unregistered)
                    {
						MsgLogger.WriteDebug($"{GetType().Name} - method", $"create device name='{device.Family}', protocol='{device.ProtocolStackName}', intf='{device.InterfaceName}', port='{device.PortName}'");
						
                        var eposDevice = EposDeviceFactory.CreateDevice(device.Family,
                            device.InterfaceName,
                            device.ProtocolStackName,
                            device.PortName, _settings.UpdateInterval, _settings.Timeout, device.NodeId);

                        eposDevice.Status = DeviceStatus.Detected;

                        lock (this)
                        {
                            if (existingDevice == null)
                            {
                                _devices.Add(eposDevice);
                            }
                        }

                        OnDeviceDetected(new ScannerDeviceStatusChangedEventArgs { Device = eposDevice });
                    }					
                }
                
                Thread.Sleep(deviceUpdateWaitTime);

            } while (!token.IsCancellationRequested);
        }
        
        private List<string> GetProtocolStackNames(string deviceName)
        {
            var result = new List<string>();

            string protocolStackName = string.Empty;
            uint errorCode = 0;
            int endOfSelection = 0;
            int startOfSelection = 1;
            int selectionResult = 1;

            try
            {
                while (endOfSelection == 0 && selectionResult > 0)
                {
                    selectionResult = VcsWrapper.Device.VcsGetProtocolStackNameSelection(deviceName, startOfSelection, ref protocolStackName, ref endOfSelection, ref errorCode);

                    if (selectionResult > 0)
                    {
                        result.Add(protocolStackName);
                    }

                    startOfSelection = 0;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - method", e);
            }

            return result;
        }

        private List<string> GetProtocolInterfaceNames(string deviceName, string protocolStackName)
        {
            var result = new List<string>();

            uint errorCode = 0;
            int endOfSelection = 0;
            int startOfSelection = 1;
            string interfaceName = string.Empty;
            int selectionResult = 1;

            try
            {
                while (endOfSelection == 0 && selectionResult > 0)
                {
                    selectionResult = VcsWrapper.Device.VcsGetInterfaceNameSelection(deviceName, protocolStackName, startOfSelection, ref interfaceName, ref endOfSelection, ref errorCode);

                    if (selectionResult > 0)
                    {
                        result.Add(interfaceName);
                    }

                    startOfSelection = 0;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - method", e);
            }

            return result;
        }

        private async Task<List<string>> GetPortNamesAsync(string deviceName, string protocolStackName, string interfaceName)
        {
            var result = await Task.Run(() => GetPortNames(deviceName, protocolStackName, interfaceName));
            
            return result;
        }

        private List<string> GetPortNames(string deviceName, string protocolStackName, string interfaceName)
        {
            var result = new List<string>();
            uint errorCode = 0;
            int endOfSelection = 0;
            int startOfSelection = 1;
            string portName = string.Empty;
            int selectionResult = 1;

            try
            {
                MsgLogger.WriteDebug($"{GetType().Name} - method", $"VcsGetPortNameSelection - enter - device = {deviceName}, protocol = {protocolStackName}, interface = {interfaceName}");

                while (endOfSelection == 0 && selectionResult > 0)
                {
                    selectionResult = VcsWrapper.Device.VcsGetPortNameSelection(deviceName, protocolStackName, interfaceName, startOfSelection, ref portName, ref endOfSelection, ref errorCode);

                    if (selectionResult > 0)
                    {
                        result.Add(portName);
                    }
                    
                    startOfSelection = 0;
                }

                MsgLogger.WriteDebug($"{GetType().Name} - method", "VcsGetPortNameSelection - leave");
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - method", e);
            }

            return result;
        }

        public void DisconnectAll()
        {
            foreach (var device in SafeDeviceArray)
            {
                if (device is EposDevice eposDevice)
                {
                    eposDevice.Disconnect();
                }
            }
        }

        #endregion
    }
}

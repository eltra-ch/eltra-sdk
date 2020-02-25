using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Events;
using EposMaster.DeviceManager.Events;
using EposMaster.DeviceManager.Identification.Factory;
using EposMaster.DeviceManager.Status;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EposMaster.DeviceManager.Identification;
using EposMaster.DeviceManager.Identification.Events;
using EltraMaster.DeviceManager.Events;

namespace EposMaster.DeviceManager.Device
{
    abstract class EposDevice : MotionControllerDevice
    {
        #region Private fields

        private StatusManager _statusManager;
        
        #endregion

        #region Constructors

        public EposDevice(string name, string deviceDescriptionFile)
            : base(name, deviceDescriptionFile)
        {
            RegisterEvents();
        }

        #endregion

        #region Event handler

        protected override void OnCloudAgentChanged()
        {
            CreateIdentification();
            CreateStatusManager();
            CreateVersion();
        }

        private void OnDeviceDescriptionStateChanged(object sender, DeviceDescriptionEventArgs e)
        {
            if(e.State == DeviceDescriptionState.Read)
            {
                Status = DeviceStatus.DescriptionAvailable;

                var msg = $"Device '{e.DeviceDescription.ProductName}' description read - Status: {Status}";

                MsgLogger.WriteLine(msg);

                if (CreateObjectDictionary())
                {
                    Status = DeviceStatus.Connected;
                }
            }
        }
        
        private void OnCommunicationStatusChanged(object sender, DeviceCommunicationEventArgs e)
        {
            var device = e.Device as EposDevice;
            var eposEventArgs = e as EposCommunicationEventArgs;
            var status = eposEventArgs?.Status;

            MsgLogger.WriteDebug($"{GetType().Name} - method", $"Device: {device.Name} ({device.PortName}) - state = {status}");

            switch (status)
            {
                case EposCommunicationStatus.Connected:
                {
                    if (_statusManager.CheckState()) //detect fake connect
                    {
                        Identification.Read();

                        Status = DeviceStatus.Connected;
                    }
                    else
                    {
                        (Communication as Epos4DeviceCommunication).Disconnect(); // fake connection -> disconnect
                    }
                } break;
                case EposCommunicationStatus.Disconnected:
                {
                    Status = DeviceStatus.Disconnected;
                } break;
                case EposCommunicationStatus.Failed:
                {
                    Status = DeviceStatus.Undefined;
                } break;
            }
        }
        
        private async void OnVersionUpdated(object sender, DeviceVersionEventArgs e)
        {
            Status = DeviceStatus.VersionAvailable;

            var msg =
                $"Device version: 0x{e.Version.HardwareVersion:X} 0x{e.Version.SoftwareVersion:X} 0x{e.Version.ApplicationNumber:X} 0x{e.Version.ApplicationVersion:X}";

            MsgLogger.WriteLine(msg);

            await DeviceDescription.Read();
        }
        
        protected void OnIdentificationStateChanged(object sender, DeviceIdentificationEventArgs e)
        {
            if (e.State == DeviceIdentificationState.Success)
            {
                Status = DeviceStatus.Identified;

                MsgLogger.WriteLine($"Identified device='{e.Device.Name}' has serial number=0x{e.SerialNumber:X}");

                (Version as EposDeviceVersion)?.Read();
            }
            else if (e.State == DeviceIdentificationState.Failed)
            {
                MsgLogger.WriteLine($"Device: {e.Device.Name} ({(e.Device as EposDevice)?.PortName}) - Identification failed, reason={e.LastErrorCode}");
            }
        }

        #endregion

        #region Methods

        private void CreateVersion()
        {
            Version = new EposDeviceVersion(this);
        }

        private void CreateStatusManager()
        {
            _statusManager = new StatusManager(this);
        }

        private void CreateIdentification()
        {
            Identification = DeviceIdentificationFactory.CreateIdentification(this);
        }
        
        private void RegisterEvents()
        {
            Communication.StatusChanged += OnCommunicationStatusChanged;

            if (Version is EposDeviceVersion eposDeviceVersion)
            {
                eposDeviceVersion.VersionUpdated += OnVersionUpdated;
            }
            
            if (Identification is EposDeviceIdentification eposDeviceIdentification)
            {
                eposDeviceIdentification.StateChanged += OnIdentificationStateChanged;
            }
            
            DeviceDescription.StateChanged += OnDeviceDescriptionStateChanged;
        }
        
        public async Task<bool> Connect()
        {
            bool result = false;

            await Task.Run(() =>
            {
                result = (Communication as Epos4DeviceCommunication).Connect();
            });

            return result;
        }

        public override void Disconnect()
        {
            if (_statusManager.IsRunning)
            {
                _statusManager.Stop();
            }

            (Communication as Epos4DeviceCommunication).Disconnect();

            base.Disconnect();
        }

        protected override void StartConnectionManagersAsync(ref List<Task> tasks)
        {
            StartStatusManager(ref tasks);
        }
        
        private void StartStatusManager(ref List<Task> tasks)
        {
            var statusManagerTask = new Task(() => _statusManager.Run());

            statusManagerTask.Start();

            tasks.Add(statusManagerTask);
        }

        #endregion
    }
}

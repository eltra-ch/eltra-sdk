using System;
using EltraCloudContracts.Contracts.Devices;
using EposMaster.DeviceManager.Device;
using EposMaster.DeviceManager.Events;

namespace EposMaster.DeviceManager
{
    class EposDeviceVersion : DeviceVersion
    {
        #region Private fields

        private readonly EposDevice _device;
        
        #endregion

        #region Constructors

        public EposDeviceVersion(EposDevice device)
        {
            _device = device;
        }

        #endregion
        
        #region Events

        public event EventHandler<DeviceVersionEventArgs> VersionUpdated;
        public event EventHandler<DeviceVersionEventArgs> VersionUpdateFailed;

        #endregion

        #region Events handling

        protected virtual void OnVersionUpdated(DeviceVersionEventArgs e)
        {
            VersionUpdated?.Invoke(this, e);
        }

        protected virtual void OnVersionUpdateFailed(DeviceVersionEventArgs e)
        {
            VersionUpdateFailed?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public void Read()
        {
            try
            {
                ushort hardwareVersion = 0;
                ushort softwareVersion = 0;
                ushort applicationNumber = 0;
                ushort applicationVersion = 0;
                uint lastErrorCode = 0;
                var communication = _device.Communication as Epos4DeviceCommunication;

                if (VcsWrapper.Device.VcsGetVersion(communication.KeyHandle, communication.NodeId, 
                                                    ref hardwareVersion, ref softwareVersion, ref applicationNumber, ref applicationVersion, 
                                                    ref lastErrorCode) > 0)
                {
                    HardwareVersion = hardwareVersion;
                    SoftwareVersion = softwareVersion;
                    ApplicationNumber = applicationNumber;
                    ApplicationVersion = applicationVersion;

                    OnVersionUpdated(new DeviceVersionEventArgs { Device = _device, Version = this, LastErrorCode = lastErrorCode });
                }
                else
                {
                    OnVersionUpdateFailed(new DeviceVersionEventArgs { Device = _device, Version = this, LastErrorCode = lastErrorCode });
                }
            }
            catch (Exception e)
            {
                OnVersionUpdateFailed(new DeviceVersionEventArgs { Device = _device, Version = this, Exception = e });
            }
        }
        public void CopyTo(DeviceVersion deviceVersion)
        {
            deviceVersion.ApplicationNumber = ApplicationNumber;
            deviceVersion.ApplicationVersion = ApplicationVersion;
            deviceVersion.HardwareVersion = HardwareVersion;
            deviceVersion.SoftwareVersion = SoftwareVersion;
        }

        #endregion
    }
}

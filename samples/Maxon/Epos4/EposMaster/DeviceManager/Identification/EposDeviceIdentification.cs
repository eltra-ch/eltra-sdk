using System;
using System.Text;
using EltraCommon.Contracts.Devices;
using EposMaster.DeviceManager.Device;
using EposMaster.DeviceManager.Identification.Events;

namespace EposMaster.DeviceManager.Identification
{
    class EposDeviceIdentification : DeviceIdentification
    {
        #region Private fields
        
        private readonly EposDevice _device;
        private uint _lastErrorCode;
        protected Exception _lastException;

        #endregion

        #region Constructors

        public EposDeviceIdentification(EposDevice device)
        {
            _device = device;
        }

        #endregion
        
        #region Properties
        
        protected EposDevice Device => _device;

        #endregion

        #region Events

        public event EventHandler<DeviceIdentificationEventArgs> StateChanged;

        #endregion

        #region Events handling

        protected virtual void OnStateChanged(DeviceIdentificationEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }
        
        #endregion


        #region Methods

        public override bool Read()
        {
            bool result = ReadSerialNumber();

            if (result)
            {
                result = ReadDeviceName();
            }

            var state = result ? DeviceIdentificationState.Success : DeviceIdentificationState.Failed;

            OnStateChanged(new DeviceIdentificationEventArgs { Device = _device, SerialNumber = SerialNumber,
                                                               LastErrorCode = _lastErrorCode,
                                                               State = state, Exception = _lastException });

            return result;
        }
        
        private bool ReadDeviceName()
        {
            bool result = false;

            try
            {
                var communication = _device?.Communication as Epos4DeviceCommunication;
                ushort maxDeviceNameSize = 255;
                StringBuilder deviceName = new StringBuilder(maxDeviceNameSize);

                if (communication != null && communication.Connected)
                {
                    if (VcsWrapper.Device.VcsGetDeviceName(communication.KeyHandle, deviceName, maxDeviceNameSize, ref _lastErrorCode) > 0)
                    {
                        Name = deviceName.ToString();

                        result = !string.IsNullOrEmpty(Name);
                    }
                }
            }
            catch (Exception e)
            {
                _lastException = e;
            }

            return result;
        }

        protected virtual bool ReadSerialNumber()
        {
            bool result = false;
            const ushort indexSerialNumber = 0x2004;
            const byte subIndexSerialNumber = 0x00;
            uint numberOfBytesRead = 8;
            byte[] data = new byte[numberOfBytesRead];
            var communication = Device?.Communication as Epos4DeviceCommunication;
            uint errorCode = 0;

            try
            {
                if (communication != null && communication.Connected)
                {
                    if (VcsWrapper.Device.VcsGetObject(
                            communication.KeyHandle,
                            communication.NodeId,
                            indexSerialNumber,
                            subIndexSerialNumber,
                            data,
                            numberOfBytesRead,
                            ref numberOfBytesRead,
                            ref errorCode) > 0)
                    {
                        SerialNumber = BitConverter.ToUInt64(data,0);

                        result = SerialNumber > 0;
                    }
                }
            }
            catch (Exception e)
            {
                _lastException = e;
            }

            return result;
        }

        #endregion

        
    }
}

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using EltraCloudContracts.Contracts.Devices;

namespace EltraMaster.Device
{
    public class MasterDeviceCommunication
    {
        #region Private fields

        private readonly MasterDevice _device;
        private MasterVcs _vcs;
        private uint _updateInterval;
        private uint _timeout;

        #endregion

        #region Constructors

        public MasterDeviceCommunication(MasterDevice device, uint updateInterval, uint timeout)
        {
            _device = device;
            _updateInterval = updateInterval;
            _timeout = timeout;

            if (_device != null)
            {
                _device.StatusChanged += OnDeviceStatusChanged;
            }
        }

        #endregion

        #region Properties

        public uint LastErrorCode { get; set; }

        protected MasterDevice Device => _device;

        protected MasterVcs Vcs
        {
            get
            {
                return _vcs ?? (_vcs = new MasterVcs(Device, _updateInterval, _timeout)); ;
            }
        }

        #endregion

        #region Events

        public event EventHandler Initialized;

        public event EventHandler<DeviceCommunicationEventArgs> StatusChanged;

        #endregion

        #region Events handler

        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, new EventArgs());
        }

        private void OnDeviceStatusChanged(object sender, EventArgs e)
        {
            if (_device.Status == DeviceStatus.Registered)
            {
                OnInitialized();
            }
        }

        protected virtual void OnStatusChanged(DeviceCommunicationEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public virtual bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            return false;
        }

        public virtual bool SetObject(ushort objectIndex, byte objectSubindex, byte[] data)
        {
            return false;
        }

        protected bool UpdateParameterDictionaryValue<T>(ushort objectIndex, byte objectSubindex, T newValue)
        {
            bool result = false;

            if (_device?.SearchParameter(objectIndex, objectSubindex) is Parameter parameter)
            {
                result = parameter.SetValue(newValue);
            }

            return result;
        }

        #endregion
    }
}

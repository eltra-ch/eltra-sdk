using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using EltraCommon.Contracts.Devices;

#pragma warning disable 1591

namespace EltraConnector.Master.Device
{
    public class MasterDeviceCommunication
    {
        #region Private fields

        private MasterVcs _vcs;
        
        #endregion

        #region Constructors

        public MasterDeviceCommunication(MasterDevice device)
        {            
            Device = device;

            if (Device != null)
            {
                Device.StatusChanged += OnDeviceStatusChanged;
            }
        }

        #endregion

        #region Properties

        public uint LastErrorCode { get; set; }

        protected MasterDevice Device { get; }

        protected MasterVcs Vcs
        {
            get
            {
                return _vcs ?? (_vcs = new MasterVcs(Device));
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
            if (Device.Status == DeviceStatus.Registered ||
                Device.Status == DeviceStatus.Ready)
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

        public virtual bool GetObject(string loginName, ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            return GetObject(objectIndex, objectSubindex, ref data);
        }

        public virtual bool SetObject(string loginName, ushort objectIndex, byte objectSubindex, byte[] data)
        {
            return SetObject(objectIndex, objectSubindex, data);
        }

        protected bool UpdateParameterDictionaryValue<T>(ushort objectIndex, byte objectSubindex, T newValue)
        {
            bool result = false;

            if (Device?.SearchParameter(objectIndex, objectSubindex) is Parameter parameter)
            {
                result = parameter.SetValue(newValue);
            }

            return result;
        }

        #endregion
    }
}

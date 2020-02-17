using System.Collections.Generic;
using System.Runtime.Serialization;

using EltraCloudContracts.Contracts.Devices;

namespace EltraCloudContracts.Contracts.Sessions
{
    [DataContract]
    public class SessionDevices
    {
        #region Private fields

        private List<EltraDevice> _deviceList;
        
        #endregion

        #region Properties

        [DataMember]
        public string SessionUuid { get; set; }

        [DataMember]
        public List<EltraDevice> Devices
        {
            get => _deviceList ?? (_deviceList = new List<EltraDevice>());
            set => _deviceList = value;
        }

        public int DevicesCount => Devices.Count;

        #endregion

        #region Methods

        public bool AddDevice(EltraDevice device)
        {
            bool result = false;

            if (!DeviceExists(device))
            {
                device.SessionUuid = SessionUuid;

                Devices.Add(device);

                result = true;
            }

            return result;
        }

        public EltraDevice FindDeviceBySerialNumber(ulong serialNumber)
        {
            EltraDevice result = null;

            foreach (var d in Devices)
            {
                if (d.Identification.SerialNumber == serialNumber)
                {
                    result = d;
                    break;
                }
            }

            return result;
        }

        public void RemoveDevice(EltraDevice device)
        {
            Devices.Remove(device);
        }

        private bool DeviceExists(EltraDevice device)
        {
            bool result = false;

            if (device?.Identification != null)
            {
                result = FindDeviceBySerialNumber(device.Identification.SerialNumber) != null;
            }
            
            return result;
        }

        #endregion
    }
}

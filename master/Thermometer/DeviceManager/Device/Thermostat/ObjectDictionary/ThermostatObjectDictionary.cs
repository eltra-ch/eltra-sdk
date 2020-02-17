using EltraCloudContracts.ObjectDictionary.Common;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription;

namespace ThermoMaster.DeviceManager.Device.Thermostat.ObjectDictionary
{
    public class ThermostatObjectDictionary : DeviceObjectDictionary
    {
        #region Constructors

        public ThermostatObjectDictionary(EltraCloudContracts.Contracts.Devices.EltraDevice device)
            : base(device)
        {            
        }

        protected override void CreateDeviceDescription()
        {
            var xdd = new Xdd(Device) { DataSource = Device.DeviceDescription.Content };

            SetDeviceDescription(xdd);
        }

        #endregion
    }
}

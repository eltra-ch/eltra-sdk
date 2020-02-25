using EltraMaster.Device;
using ThermoMaster.DeviceManager.Device;
using ThermoMaster.Settings;

namespace ThermoMaster.DeviceManager
{
    class ThermoDeviceManager : MasterDeviceManager
    {
        public ThermoDeviceManager(MasterSettings settings)
        {
            AddDevice(new ThermoDevice(settings));
        }
    }
}

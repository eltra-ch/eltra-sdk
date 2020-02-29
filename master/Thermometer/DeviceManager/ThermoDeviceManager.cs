using EltraMaster.Device;
using ThermoMaster.Settings;
using Thermometer.DeviceManager.Device;

namespace Thermometer.DeviceManager
{
    public class ThermoDeviceManager : MasterDeviceManager
    {
        public ThermoDeviceManager(MasterSettings settings)
        {
            AddDevice(new ThermoDevice(settings));
        }
    }
}

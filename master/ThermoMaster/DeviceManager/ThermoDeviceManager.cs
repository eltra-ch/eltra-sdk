using EltraMaster.Device;
using ThermoMaster.DeviceManager.Device;
using ThermoMaster.DeviceManager.Wrapper;
using ThermoMaster.Settings;

namespace ThermoMaster.DeviceManager
{
    public class ThermoDeviceManager : MasterDeviceManager
    {
        public ThermoDeviceManager(MasterSettings settings)
        {
            EltraBmp180Wrapper.Initialize();
            EltraDht22Wrapper.Initialize();
            EltraRelayWrapper.Initialize();
            EltraSds011Wrapper.Initialize();

            AddDevice(new ThermoDevice(settings));
        }

        protected override void Dispose(bool finalize)
        {
            base.Dispose(finalize);

            EltraBmp180Wrapper.Release();
            EltraRelayWrapper.Release();
            EltraDht22Wrapper.Release();
            EltraSds011Wrapper.Release();
        }
    }
}

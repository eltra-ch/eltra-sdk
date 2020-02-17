using EltraConnector.UserAgent.Vcs;

namespace ThermoMaster.DeviceManager.Device
{
    class ThermoVcs : DeviceVcs
    {
        public ThermoVcs(ThermoDeviceBase device, uint updateInterval, uint timeout)
            : base(device.CloudAgent.Url, device.SessionUuid, device.CloudAgent.AuthData, updateInterval, timeout)
        {
            Device = device;
        }
    }
}

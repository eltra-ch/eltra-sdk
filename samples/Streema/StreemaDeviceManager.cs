using EltraConnector.Master.Device;

namespace StreemaMaster
{
    class StreemaDeviceManager : MasterDeviceManager
    {
        public StreemaDeviceManager(string deviceDescriptionFilePath, StreemaSettings settings)
        {
            AddDevice(new StreemaDevice(deviceDescriptionFilePath, 1, settings));
        }
    }
}

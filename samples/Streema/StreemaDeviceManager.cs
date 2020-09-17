using EltraConnector.Master.Device;

namespace StreemaMaster
{
    class StreemaDeviceManager : MasterDeviceManager
    {
        public StreemaDeviceManager(string deviceDescriptionFilePath)
        {
            AddDevice(new StreemaDevice(deviceDescriptionFilePath, 1));
        }
    }
}

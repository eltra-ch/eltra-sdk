using EltraConnector.Master.Device;

namespace MPlayerMaster.Device
{
    class MPlayerDeviceManager : MasterDeviceManager
    {
        public MPlayerDeviceManager(string deviceDescriptionFilePath, MPlayerSettings settings)
        {
            AddDevice(new MPlayerDevice(deviceDescriptionFilePath, 1, settings));
        }
    }
}

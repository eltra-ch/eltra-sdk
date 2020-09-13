using EltraConnector.Master.Device;

namespace ConsoleApp1
{
    class DummyDeviceManager : MasterDeviceManager
    {
        public DummyDeviceManager(string deviceDescriptionFilePath)
        {
            AddDevice(new DummyDevice(deviceDescriptionFilePath, 1));
            AddDevice(new DummyDevice(deviceDescriptionFilePath, 2));
        }
    }
}

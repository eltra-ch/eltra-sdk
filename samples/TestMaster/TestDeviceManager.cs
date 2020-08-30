using EltraConnector.Master.Device;

namespace TestMaster
{
    class TestDeviceManager : MasterDeviceManager
    {
        public TestDeviceManager(string deviceDescriptionFilePath)
        {
            AddDevice(new TestDevice(deviceDescriptionFilePath, 1));
            AddDevice(new TestDevice(deviceDescriptionFilePath, 2));
            AddDevice(new TestDevice(deviceDescriptionFilePath, 3));
        }
    }
}

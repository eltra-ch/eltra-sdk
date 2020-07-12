using EltraConnector.Master.Device;

namespace ConsoleApp1
{
    class DummyDeviceManager : MasterDeviceManager
    {
        public DummyDeviceManager(string deviceDescriptionFilePath)
        {
            int nodeId = 1;
            
            AddDevice(new DummyDevice(deviceDescriptionFilePath, nodeId));
        }
    }
}

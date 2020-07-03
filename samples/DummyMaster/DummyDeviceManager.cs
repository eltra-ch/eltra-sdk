using EltraConnector.Master.Device;

namespace ConsoleApp1
{
    class DummyDeviceManager : MasterDeviceManager
    {
        public DummyDeviceManager(string xdd)
        {
            AddDevice(new DummyDevice(xdd));
        }
    }
}

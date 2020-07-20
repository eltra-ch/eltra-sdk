using EltraConnector.Master.Device;
using System;

namespace ConsoleApp1
{
    internal class DummyDevice : MasterDevice
    {
        public DummyDevice(string deviceDescriptionFilePath, int nodeId) 
            : base("DUMMY", deviceDescriptionFilePath, nodeId)
        {
            Identification.SerialNumber = 0x105;

            AddCommand(new StartCountingCommand(this));
            AddCommand(new StopCountingCommand(this));
        }

        protected override void OnStatusChanged()
        {
            Console.WriteLine($"device status changed: new status = {Status}");

            base.OnStatusChanged();
        }

        protected override void CreateCommunication()
        {
            var communication = new DummyDeviceCommunication(this);

            Communication = communication;
        }
    }
}

using EltraConnector.Master.Device;
using System;

namespace TestMaster
{
    internal class TestDevice : MasterDevice
    {
        public TestDevice(string deviceDescriptionFilePath, int nodeId) 
            : base("TEST", deviceDescriptionFilePath, nodeId)
        {
            Identification.SerialNumber = 0x107;

            AddCommand(new StartCountingCommand(this));
            AddCommand(new StopCountingCommand(this));
        }

        protected override void OnStatusChanged()
        {
            Console.WriteLine($"device (node id = {NodeId}) status changed: new status = {Status}");

            base.OnStatusChanged();
        }

        protected override void CreateCommunication()
        {
            var communication = new TestDeviceCommunication(this);

            Communication = communication;
        }
    }
}

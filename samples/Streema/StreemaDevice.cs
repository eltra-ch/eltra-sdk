using EltraConnector.Master.Device;
using System;

namespace StreemaMaster
{
    internal class StreemaDevice : MasterDevice
    {
        public StreemaDevice(string deviceDescriptionFilePath, int nodeId) 
            : base("STREEMA", deviceDescriptionFilePath, nodeId)
        {
            Identification.SerialNumber = 0x100;
        }

        protected override void OnStatusChanged()
        {
            Console.WriteLine($"device (node id = {NodeId}) status changed: new status = {Status}");

            base.OnStatusChanged();
        }

        protected override void CreateCommunication()
        {
            var communication = new StreemaDeviceCommunication(this);

            Communication = communication;
        }
    }
}

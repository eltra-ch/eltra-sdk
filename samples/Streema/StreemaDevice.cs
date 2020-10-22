using EltraConnector.Master.Device;
using System;

namespace StreemaMaster
{
    internal class StreemaDevice : MasterDevice
    {
        private StreemaSettings _settings;

        public StreemaDevice(string deviceDescriptionFilePath, int nodeId, StreemaSettings settings) 
            : base("STREEMA", deviceDescriptionFilePath, nodeId)
        {
            _settings = settings;

            Identification.SerialNumber = 0x100;
        }

        protected override void OnStatusChanged()
        {
            Console.WriteLine($"device (node id = {NodeId}) status changed: new status = {Status}");

            base.OnStatusChanged();
        }

        protected override void CreateCommunication()
        {
            var communication = new StreemaDeviceCommunication(this, _settings);

            Communication = communication;
        }
    }
}

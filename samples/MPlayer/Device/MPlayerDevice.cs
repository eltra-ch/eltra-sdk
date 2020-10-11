using EltraConnector.Master.Device;
using System;

namespace MPlayerMaster
{
    internal class MPlayerDevice : MasterDevice
    {
        private MPlayerSettings _settings;

        public MPlayerDevice(string deviceDescriptionFilePath, int nodeId, MPlayerSettings settings) 
            : base("MPLAYER", deviceDescriptionFilePath, nodeId)
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
            var communication = new MPlayerDeviceCommunication(this, _settings);

            Communication = communication;
        }
    }
}

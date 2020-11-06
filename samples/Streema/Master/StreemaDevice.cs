using EltraCommon.Contracts.ToolSet;
using EltraConnector.Master.Device;
using System;
using System.IO;

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

        protected override bool UpdatePayloadContent(DeviceToolPayload payload)
        {
            bool result = false;

            if (payload.FileName == "EltraNavigoStreema.dll")
            {
                string path = Path.Combine(Environment.CurrentDirectory, _settings.NavigoPluginsPath, "EltraNavigoMPlayer.dll");

                result = UpdatePayloadFromFile(path, payload);
            }

            return result;
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

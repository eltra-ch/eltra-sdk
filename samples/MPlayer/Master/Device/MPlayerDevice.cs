using EltraCommon.Contracts.Parameters;
using EltraConnector.Master.Device;
using System;
using System.IO;
using EltraCommon.Contracts.ToolSet;
using MPlayerMaster.Device.Commands;

namespace MPlayerMaster.Device
{
    internal class MPlayerDevice : MasterDevice
    {
        private MPlayerSettings _settings;

        public MPlayerDevice(string deviceDescriptionFilePath, int nodeId, MPlayerSettings settings) 
            : base("MPLAYER", deviceDescriptionFilePath, nodeId)
        {
            _settings = settings;

            Identification.SerialNumber = 0x102;

            AddCommand(new QueryStationCommand(this));
        }

        protected override bool UpdatePayloadContent(DeviceToolPayload payload)
        {
            string path = Path.Combine(Environment.CurrentDirectory, _settings.NavigoPluginsPath, payload.FileName);

            bool result = UpdatePayloadFromFile(path, payload);

            return result;
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

        public override int GetUpdateInterval(ParameterUpdatePriority priority)
        {
            int result;

            switch (priority)
            {
                case ParameterUpdatePriority.High:
                    result = 500;
                    break;
                case ParameterUpdatePriority.Medium:
                    result = 750;
                    break;
                case ParameterUpdatePriority.Low:
                    result = 1000;
                    break;
                case ParameterUpdatePriority.Lowest:
                    result = 3000;
                    break;
                default:
                    result = 1000;
                    break;
            }

            return result;
        }
    }
}

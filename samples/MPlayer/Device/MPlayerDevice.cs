using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.ToolSet;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraConnector.Master.Device;
using System;
using System.IO;
using System.Threading.Tasks;

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

        protected override bool UpdatePayloadContent(DeviceToolPayload payload)
        {
            bool result = base.UpdatePayloadContent(payload);
            string path = Path.Combine(_settings.NavigoPluginsPath, payload.FileName);

            if (File.Exists(path))
            {
                var bytes = File.ReadAllBytes(path);

                payload.Content = Convert.ToBase64String(bytes);
                payload.HashCode = CryptHelpers.ToMD5(payload.Content);
                
                result = true;
            }

            return result;
        }
    }
}

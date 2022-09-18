using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class UpdateFirmwareCommand : DeviceCommand
	{
		public UpdateFirmwareCommand(EltraDevice device)
			:base(device)
		{
			Name = "UpdateFirmware";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

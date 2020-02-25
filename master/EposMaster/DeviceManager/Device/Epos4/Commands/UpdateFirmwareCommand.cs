using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class UpdateFirmwareCommand : DeviceCommand
	{
		public UpdateFirmwareCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "UpdateFirmware";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

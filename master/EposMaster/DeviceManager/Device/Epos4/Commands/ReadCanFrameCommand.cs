using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadCanFrameCommand : DeviceCommand
	{
		public ReadCanFrameCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ReadCanFrame";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

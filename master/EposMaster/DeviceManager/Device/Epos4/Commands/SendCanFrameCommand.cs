using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SendCanFrameCommand : DeviceCommand
	{
		public SendCanFrameCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SendCanFrame";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

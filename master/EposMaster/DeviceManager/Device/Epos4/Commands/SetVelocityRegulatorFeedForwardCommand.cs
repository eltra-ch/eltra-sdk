using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetVelocityRegulatorFeedForwardCommand : DeviceCommand
	{
		public SetVelocityRegulatorFeedForwardCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetVelocityRegulatorFeedForward";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

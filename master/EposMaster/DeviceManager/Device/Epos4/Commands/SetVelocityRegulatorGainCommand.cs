using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetVelocityRegulatorGainCommand : DeviceCommand
	{
		public SetVelocityRegulatorGainCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetVelocityRegulatorGain";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

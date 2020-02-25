using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetVelocityRegulatorGainCommand : DeviceCommand
	{
		public GetVelocityRegulatorGainCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetVelocityRegulatorGain";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

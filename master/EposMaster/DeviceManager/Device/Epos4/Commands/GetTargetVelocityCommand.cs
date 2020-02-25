using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetTargetVelocityCommand : DeviceCommand
	{
		public GetTargetVelocityCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetTargetVelocity";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

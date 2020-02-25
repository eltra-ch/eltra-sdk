using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetMovementStateCommand : DeviceCommand
	{
		public GetMovementStateCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetMovementState";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

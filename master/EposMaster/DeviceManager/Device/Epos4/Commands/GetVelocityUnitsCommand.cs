using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetVelocityUnitsCommand : DeviceCommand
	{
		public GetVelocityUnitsCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetVelocityUnits";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

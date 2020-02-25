using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class StopIpmTrajectoryCommand : DeviceCommand
	{
		public StopIpmTrajectoryCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "StopIpmTrajectory";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class StopIpmTrajectoryCommand : DeviceCommand
	{
		public StopIpmTrajectoryCommand(EltraDevice device)
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

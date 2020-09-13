using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class StartIpmTrajectoryCommand : DeviceCommand
	{
		public StartIpmTrajectoryCommand(EltraDevice device)
			:base(device)
		{
			Name = "StartIpmTrajectory";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

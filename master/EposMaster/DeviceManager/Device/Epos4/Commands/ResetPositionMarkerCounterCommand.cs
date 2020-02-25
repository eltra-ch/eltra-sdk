using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ResetPositionMarkerCounterCommand : DeviceCommand
	{
		public ResetPositionMarkerCounterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ResetPositionMarkerCounter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

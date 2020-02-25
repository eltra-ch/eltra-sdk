using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivatePositionCompareCommand : DeviceCommand
	{
		public DeactivatePositionCompareCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DeactivatePositionCompare";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

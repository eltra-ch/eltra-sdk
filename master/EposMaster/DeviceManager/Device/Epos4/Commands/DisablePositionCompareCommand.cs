using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisablePositionCompareCommand : DeviceCommand
	{
		public DisablePositionCompareCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DisablePositionCompare";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

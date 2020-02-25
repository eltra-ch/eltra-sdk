using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class EnablePositionCompareCommand : DeviceCommand
	{
		public EnablePositionCompareCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "EnablePositionCompare";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

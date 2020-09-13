using System;
using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivatePositionCompareCommand : DeviceCommand
	{
		public ActivatePositionCompareCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivatePositionCompare";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

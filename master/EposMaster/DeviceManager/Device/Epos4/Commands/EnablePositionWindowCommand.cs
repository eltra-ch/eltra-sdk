using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class EnablePositionWindowCommand : DeviceCommand
	{
		public EnablePositionWindowCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "EnablePositionWindow";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

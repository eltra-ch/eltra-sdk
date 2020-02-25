using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisablePositionWindowCommand : DeviceCommand
	{
		public DisablePositionWindowCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DisablePositionWindow";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

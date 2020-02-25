using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisableAllTriggersCommand : DeviceCommand
	{
		public DisableAllTriggersCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DisableAllTriggers";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

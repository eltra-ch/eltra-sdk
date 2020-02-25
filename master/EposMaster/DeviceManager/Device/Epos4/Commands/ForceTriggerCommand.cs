using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ForceTriggerCommand : DeviceCommand
	{
		public ForceTriggerCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ForceTrigger";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

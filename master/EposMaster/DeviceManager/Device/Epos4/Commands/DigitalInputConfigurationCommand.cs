using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DigitalInputConfigurationCommand : DeviceCommand
	{
		public DigitalInputConfigurationCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DigitalInputConfiguration";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

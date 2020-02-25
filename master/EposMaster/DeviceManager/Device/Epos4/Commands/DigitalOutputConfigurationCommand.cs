using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DigitalOutputConfigurationCommand : DeviceCommand
	{
		public DigitalOutputConfigurationCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DigitalOutputConfiguration";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

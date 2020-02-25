using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class AnalogOutputConfigurationCommand : DeviceCommand
	{
		public AnalogOutputConfigurationCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "AnalogOutputConfiguration";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

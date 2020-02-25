using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class AnalogInputConfigurationCommand : DeviceCommand
	{
		public AnalogInputConfigurationCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "AnalogInputConfiguration";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

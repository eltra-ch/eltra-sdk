using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisableAnalogCurrentSetpointCommand : DeviceCommand
	{
		public DisableAnalogCurrentSetpointCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DisableAnalogCurrentSetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

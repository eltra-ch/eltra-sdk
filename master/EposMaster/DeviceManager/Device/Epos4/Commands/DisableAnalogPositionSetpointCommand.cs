using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisableAnalogPositionSetpointCommand : DeviceCommand
	{
		public DisableAnalogPositionSetpointCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DisableAnalogPositionSetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

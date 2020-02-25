using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisableAnalogVelocitySetpointCommand : DeviceCommand
	{
		public DisableAnalogVelocitySetpointCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DisableAnalogVelocitySetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

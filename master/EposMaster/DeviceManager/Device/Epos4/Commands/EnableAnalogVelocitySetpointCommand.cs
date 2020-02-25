using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class EnableAnalogVelocitySetpointCommand : DeviceCommand
	{
		public EnableAnalogVelocitySetpointCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "EnableAnalogVelocitySetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

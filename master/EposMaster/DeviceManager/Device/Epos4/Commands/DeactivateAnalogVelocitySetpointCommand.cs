using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivateAnalogVelocitySetpointCommand : DeviceCommand
	{
		public DeactivateAnalogVelocitySetpointCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DeactivateAnalogVelocitySetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

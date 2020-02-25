using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivateAnalogPositionSetpointCommand : DeviceCommand
	{
		public DeactivateAnalogPositionSetpointCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DeactivateAnalogPositionSetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

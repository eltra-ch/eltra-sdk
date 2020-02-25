using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivateAnalogCurrentSetpointCommand : DeviceCommand
	{
		public DeactivateAnalogCurrentSetpointCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DeactivateAnalogCurrentSetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivateAnalogVelocitySetpointCommand : DeviceCommand
	{
		public ActivateAnalogVelocitySetpointCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ActivateAnalogVelocitySetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

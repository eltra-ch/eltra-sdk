using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivateAnalogPositionSetpointCommand : DeviceCommand
	{
		public ActivateAnalogPositionSetpointCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivateAnalogPositionSetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivateAnalogPositionSetpointCommand : DeviceCommand
	{
		public DeactivateAnalogPositionSetpointCommand(EltraDevice device)
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

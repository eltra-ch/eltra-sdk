using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

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

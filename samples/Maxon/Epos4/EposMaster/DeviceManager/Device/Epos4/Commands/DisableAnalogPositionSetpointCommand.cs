using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisableAnalogPositionSetpointCommand : DeviceCommand
	{
		public DisableAnalogPositionSetpointCommand(EltraDevice device)
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

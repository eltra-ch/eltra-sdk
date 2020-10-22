using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class EnableAnalogPositionSetpointCommand : DeviceCommand
	{
		public EnableAnalogPositionSetpointCommand(EltraDevice device)
			:base(device)
		{
			Name = "EnableAnalogPositionSetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

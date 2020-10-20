using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class EnableAnalogVelocitySetpointCommand : DeviceCommand
	{
		public EnableAnalogVelocitySetpointCommand(EltraDevice device)
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

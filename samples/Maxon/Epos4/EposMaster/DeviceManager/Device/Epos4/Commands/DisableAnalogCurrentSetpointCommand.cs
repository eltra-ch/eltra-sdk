using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisableAnalogCurrentSetpointCommand : DeviceCommand
	{
		public DisableAnalogCurrentSetpointCommand(EltraDevice device)
			:base(device)
		{
			Name = "DisableAnalogCurrentSetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

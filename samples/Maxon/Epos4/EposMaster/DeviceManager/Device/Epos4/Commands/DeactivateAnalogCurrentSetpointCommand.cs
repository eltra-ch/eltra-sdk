using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivateAnalogCurrentSetpointCommand : DeviceCommand
	{
		public DeactivateAnalogCurrentSetpointCommand(EltraDevice device)
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

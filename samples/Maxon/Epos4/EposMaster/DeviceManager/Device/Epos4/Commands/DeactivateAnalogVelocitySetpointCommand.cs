using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivateAnalogVelocitySetpointCommand : DeviceCommand
	{
		public DeactivateAnalogVelocitySetpointCommand(EltraDevice device)
			:base(device)
		{
			Name = "DeactivateAnalogVelocitySetpoint";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

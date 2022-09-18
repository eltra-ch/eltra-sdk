using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivateAnalogVelocitySetpointCommand : DeviceCommand
	{
		public ActivateAnalogVelocitySetpointCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivateAnalogVelocitySetpoint";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

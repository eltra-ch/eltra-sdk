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

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

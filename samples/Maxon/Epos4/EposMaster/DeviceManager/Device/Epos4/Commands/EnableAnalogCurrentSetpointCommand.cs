using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class EnableAnalogCurrentSetpointCommand : DeviceCommand
	{
		public EnableAnalogCurrentSetpointCommand(EltraDevice device)
			:base(device)
		{
			Name = "EnableAnalogCurrentSetpoint";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

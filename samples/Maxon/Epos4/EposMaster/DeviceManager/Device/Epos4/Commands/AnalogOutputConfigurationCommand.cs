using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class AnalogOutputConfigurationCommand : DeviceCommand
	{
		public AnalogOutputConfigurationCommand(EltraDevice device)
			:base(device)
		{
			Name = "AnalogOutputConfiguration";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

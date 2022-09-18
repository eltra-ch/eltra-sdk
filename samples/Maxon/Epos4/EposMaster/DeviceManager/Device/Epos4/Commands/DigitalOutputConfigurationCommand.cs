using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DigitalOutputConfigurationCommand : DeviceCommand
	{
		public DigitalOutputConfigurationCommand(EltraDevice device)
			:base(device)
		{
			Name = "DigitalOutputConfiguration";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

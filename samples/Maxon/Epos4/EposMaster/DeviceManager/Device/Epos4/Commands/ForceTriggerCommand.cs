using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ForceTriggerCommand : DeviceCommand
	{
		public ForceTriggerCommand(EltraDevice device)
			:base(device)
		{
			Name = "ForceTrigger";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

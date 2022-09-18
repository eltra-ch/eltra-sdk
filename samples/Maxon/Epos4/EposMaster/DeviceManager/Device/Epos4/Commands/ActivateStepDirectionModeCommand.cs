using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivateStepDirectionModeCommand : DeviceCommand
	{
		public ActivateStepDirectionModeCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivateStepDirectionMode";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

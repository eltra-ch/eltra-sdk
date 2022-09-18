using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetPositionRegulatorFeedForwardCommand : DeviceCommand
	{
		public SetPositionRegulatorFeedForwardCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetPositionRegulatorFeedForward";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

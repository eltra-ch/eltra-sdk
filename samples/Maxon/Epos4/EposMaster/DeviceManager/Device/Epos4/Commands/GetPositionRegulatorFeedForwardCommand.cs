using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetPositionRegulatorFeedForwardCommand : DeviceCommand
	{
		public GetPositionRegulatorFeedForwardCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetPositionRegulatorFeedForward";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

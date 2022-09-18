using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetVelocityRegulatorFeedForwardCommand : DeviceCommand
	{
		public GetVelocityRegulatorFeedForwardCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetVelocityRegulatorFeedForward";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

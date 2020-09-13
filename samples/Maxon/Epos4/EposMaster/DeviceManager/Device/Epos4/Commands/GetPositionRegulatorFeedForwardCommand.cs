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

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

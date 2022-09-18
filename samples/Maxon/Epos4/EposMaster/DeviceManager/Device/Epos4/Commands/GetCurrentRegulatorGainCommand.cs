using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetCurrentRegulatorGainCommand : DeviceCommand
	{
		public GetCurrentRegulatorGainCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetCurrentRegulatorGain";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

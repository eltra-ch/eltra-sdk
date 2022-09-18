using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetCurrentRegulatorGainCommand : DeviceCommand
	{
		public SetCurrentRegulatorGainCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetCurrentRegulatorGain";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

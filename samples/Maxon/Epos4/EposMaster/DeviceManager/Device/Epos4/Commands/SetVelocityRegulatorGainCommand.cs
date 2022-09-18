using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetVelocityRegulatorGainCommand : DeviceCommand
	{
		public SetVelocityRegulatorGainCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetVelocityRegulatorGain";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

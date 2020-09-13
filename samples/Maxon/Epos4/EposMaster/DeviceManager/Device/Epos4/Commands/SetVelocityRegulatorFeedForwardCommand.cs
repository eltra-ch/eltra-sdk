using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetVelocityRegulatorFeedForwardCommand : DeviceCommand
	{
		public SetVelocityRegulatorFeedForwardCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetVelocityRegulatorFeedForward";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

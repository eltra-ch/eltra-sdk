using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetPositionRegulatorGainCommand : DeviceCommand
	{
		public SetPositionRegulatorGainCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetPositionRegulatorGain";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetPositionRegulatorGainCommand : DeviceCommand
	{
		public SetPositionRegulatorGainCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
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

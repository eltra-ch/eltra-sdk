using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetPositionRegulatorGainCommand : DeviceCommand
	{
		public GetPositionRegulatorGainCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetPositionRegulatorGain";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetPositionRegulatorGainCommand : DeviceCommand
	{
		public GetPositionRegulatorGainCommand(EltraDevice device)
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

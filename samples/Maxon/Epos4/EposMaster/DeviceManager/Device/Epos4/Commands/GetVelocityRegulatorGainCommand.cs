using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetVelocityRegulatorGainCommand : DeviceCommand
	{
		public GetVelocityRegulatorGainCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetVelocityRegulatorGain";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

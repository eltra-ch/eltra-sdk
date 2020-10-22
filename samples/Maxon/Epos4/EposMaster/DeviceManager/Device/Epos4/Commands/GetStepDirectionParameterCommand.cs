using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetStepDirectionParameterCommand : DeviceCommand
	{
		public GetStepDirectionParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetStepDirectionParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

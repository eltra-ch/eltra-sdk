using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetStepDirectionParameterCommand : DeviceCommand
	{
		public GetStepDirectionParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
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

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetStepDirectionParameterCommand : DeviceCommand
	{
		public SetStepDirectionParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetStepDirectionParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetStepDirectionParameterCommand : DeviceCommand
	{
		public SetStepDirectionParameterCommand(EltraDevice device)
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

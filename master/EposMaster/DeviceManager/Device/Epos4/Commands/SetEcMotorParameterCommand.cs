using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetEcMotorParameterCommand : DeviceCommand
	{
		public SetEcMotorParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetEcMotorParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetMotorParameterCommand : DeviceCommand
	{
		public SetMotorParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetMotorParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetDcMotorParameterCommand : DeviceCommand
	{
		public SetDcMotorParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetDcMotorParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

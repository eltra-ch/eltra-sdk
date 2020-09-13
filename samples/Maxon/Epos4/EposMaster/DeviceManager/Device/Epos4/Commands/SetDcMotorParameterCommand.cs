using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetDcMotorParameterCommand : DeviceCommand
	{
		public SetDcMotorParameterCommand(EltraDevice device)
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

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetEcMotorParameterCommand : DeviceCommand
	{
		public SetEcMotorParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetEcMotorParameter";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

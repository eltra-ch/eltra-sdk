using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetEcMotorParameterCommand : DeviceCommand
	{
		public GetEcMotorParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetEcMotorParameter";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

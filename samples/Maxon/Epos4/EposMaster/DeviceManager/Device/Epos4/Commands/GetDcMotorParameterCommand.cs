using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetDcMotorParameterCommand : DeviceCommand
	{
		public GetDcMotorParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetDcMotorParameter";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

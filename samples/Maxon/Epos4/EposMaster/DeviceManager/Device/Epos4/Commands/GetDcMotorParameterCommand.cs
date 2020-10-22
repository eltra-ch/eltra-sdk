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

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

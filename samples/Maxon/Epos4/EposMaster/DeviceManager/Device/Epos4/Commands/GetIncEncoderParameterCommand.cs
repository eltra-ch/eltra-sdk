using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetIncEncoderParameterCommand : DeviceCommand
	{
		public GetIncEncoderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetIncEncoderParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

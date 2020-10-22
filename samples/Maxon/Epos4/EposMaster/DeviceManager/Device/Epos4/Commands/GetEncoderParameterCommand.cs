using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetEncoderParameterCommand : DeviceCommand
	{
		public GetEncoderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetEncoderParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

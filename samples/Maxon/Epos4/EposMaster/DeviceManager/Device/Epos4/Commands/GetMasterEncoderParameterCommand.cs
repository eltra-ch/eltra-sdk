using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetMasterEncoderParameterCommand : DeviceCommand
	{
		public GetMasterEncoderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetMasterEncoderParameter";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

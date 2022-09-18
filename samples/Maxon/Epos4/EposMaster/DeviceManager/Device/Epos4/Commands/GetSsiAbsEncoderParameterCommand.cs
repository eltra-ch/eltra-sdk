using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetSsiAbsEncoderParameterCommand : DeviceCommand
	{
		public GetSsiAbsEncoderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetSsiAbsEncoderParameter";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

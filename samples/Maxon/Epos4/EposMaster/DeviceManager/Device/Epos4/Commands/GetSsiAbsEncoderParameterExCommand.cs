using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetSsiAbsEncoderParameterExCommand : DeviceCommand
	{
		public GetSsiAbsEncoderParameterExCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetSsiAbsEncoderParameterEx";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

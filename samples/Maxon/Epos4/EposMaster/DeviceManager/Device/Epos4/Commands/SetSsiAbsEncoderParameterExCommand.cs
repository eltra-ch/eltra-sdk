using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetSsiAbsEncoderParameterExCommand : DeviceCommand
	{
		public SetSsiAbsEncoderParameterExCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetSsiAbsEncoderParameterEx";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

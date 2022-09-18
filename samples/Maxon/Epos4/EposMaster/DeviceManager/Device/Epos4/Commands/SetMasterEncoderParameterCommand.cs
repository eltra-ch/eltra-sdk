using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetMasterEncoderParameterCommand : DeviceCommand
	{
		public SetMasterEncoderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetMasterEncoderParameter";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

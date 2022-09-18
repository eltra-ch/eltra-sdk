using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetIncEncoderParameterCommand : DeviceCommand
	{
		public SetIncEncoderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetIncEncoderParameter";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetEncoderParameterCommand : DeviceCommand
	{
		public SetEncoderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetEncoderParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

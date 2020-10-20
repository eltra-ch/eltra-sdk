using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetSsiAbsEncoderParameterCommand : DeviceCommand
	{
		public SetSsiAbsEncoderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetSsiAbsEncoderParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

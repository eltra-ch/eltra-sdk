using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetIncEncoderParameterCommand : DeviceCommand
	{
		public SetIncEncoderParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetIncEncoderParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

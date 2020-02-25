using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetMasterEncoderParameterCommand : DeviceCommand
	{
		public SetMasterEncoderParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetMasterEncoderParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetMasterEncoderParameterCommand : DeviceCommand
	{
		public GetMasterEncoderParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetMasterEncoderParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

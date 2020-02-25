using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetSsiAbsEncoderParameterCommand : DeviceCommand
	{
		public GetSsiAbsEncoderParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetSsiAbsEncoderParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

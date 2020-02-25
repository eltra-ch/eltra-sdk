using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetSsiAbsEncoderParameterExCommand : DeviceCommand
	{
		public GetSsiAbsEncoderParameterExCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetSsiAbsEncoderParameterEx";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetSsiAbsEncoderParameterExCommand : DeviceCommand
	{
		public SetSsiAbsEncoderParameterExCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetSsiAbsEncoderParameterEx";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

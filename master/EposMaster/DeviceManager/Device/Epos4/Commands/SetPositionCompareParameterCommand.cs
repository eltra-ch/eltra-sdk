using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetPositionCompareParameterCommand : DeviceCommand
	{
		public SetPositionCompareParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetPositionCompareParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

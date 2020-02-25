using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ImportParameterCommand : DeviceCommand
	{
		public ImportParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ImportParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetSensorTypeCommand : DeviceCommand
	{
		public GetSensorTypeCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetSensorType";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

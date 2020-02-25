using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetSensorTypeCommand : DeviceCommand
	{
		public SetSensorTypeCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetSensorType";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

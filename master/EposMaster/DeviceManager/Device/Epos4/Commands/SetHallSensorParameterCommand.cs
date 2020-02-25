using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetHallSensorParameterCommand : DeviceCommand
	{
		public SetHallSensorParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetHallSensorParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

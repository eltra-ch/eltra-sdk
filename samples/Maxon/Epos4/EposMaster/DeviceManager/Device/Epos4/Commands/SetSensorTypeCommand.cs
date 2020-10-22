using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetSensorTypeCommand : DeviceCommand
	{
		public SetSensorTypeCommand(EltraDevice device)
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

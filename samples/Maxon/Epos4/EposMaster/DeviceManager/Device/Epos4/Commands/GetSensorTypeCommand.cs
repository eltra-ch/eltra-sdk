using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetSensorTypeCommand : DeviceCommand
	{
		public GetSensorTypeCommand(EltraDevice device)
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

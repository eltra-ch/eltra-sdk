using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetHallSensorParameterCommand : DeviceCommand
	{
		public SetHallSensorParameterCommand(EltraDevice device)
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

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

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

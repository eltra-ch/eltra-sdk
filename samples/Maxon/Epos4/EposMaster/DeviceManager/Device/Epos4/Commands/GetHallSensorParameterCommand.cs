using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetHallSensorParameterCommand : DeviceCommand
	{
		public GetHallSensorParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetHallSensorParameter";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

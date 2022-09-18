using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadPositionMarkerCapturedPositionCommand : DeviceCommand
	{
		public ReadPositionMarkerCapturedPositionCommand(EltraDevice device)
			:base(device)
		{
			Name = "ReadPositionMarkerCapturedPosition";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadCanFrameCommand : DeviceCommand
	{
		public ReadCanFrameCommand(EltraDevice device)
			:base(device)
		{
			Name = "ReadCanFrame";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

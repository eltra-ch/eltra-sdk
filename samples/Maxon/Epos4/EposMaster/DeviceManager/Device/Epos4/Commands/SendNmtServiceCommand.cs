using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SendNmtServiceCommand : DeviceCommand
	{
		public SendNmtServiceCommand(EltraDevice device)
			:base(device)
		{
			Name = "SendNmtService";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

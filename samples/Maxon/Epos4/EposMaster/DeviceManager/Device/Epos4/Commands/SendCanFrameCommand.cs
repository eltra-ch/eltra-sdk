using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SendCanFrameCommand : DeviceCommand
	{
		public SendCanFrameCommand(EltraDevice device)
			:base(device)
		{
			Name = "SendCanFrame";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

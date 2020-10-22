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

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

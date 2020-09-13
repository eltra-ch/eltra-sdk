using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class RequestCanFrameCommand : DeviceCommand
	{
		public RequestCanFrameCommand(EltraDevice device)
			:base(device)
		{
			Name = "RequestCanFrame";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

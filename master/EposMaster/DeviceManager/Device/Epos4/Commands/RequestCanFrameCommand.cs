using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class RequestCanFrameCommand : DeviceCommand
	{
		public RequestCanFrameCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
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

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetVersionCommand : DeviceCommand
	{
		public GetVersionCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetVersion";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

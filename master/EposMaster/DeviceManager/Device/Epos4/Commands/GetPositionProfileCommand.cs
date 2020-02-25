using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetPositionProfileCommand : DeviceCommand
	{
		public GetPositionProfileCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetPositionProfile";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

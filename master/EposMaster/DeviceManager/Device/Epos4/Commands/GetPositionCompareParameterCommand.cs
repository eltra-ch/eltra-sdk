using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetPositionCompareParameterCommand : DeviceCommand
	{
		public GetPositionCompareParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetPositionCompareParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
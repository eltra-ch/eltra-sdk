using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetPositionMarkerParameterCommand : DeviceCommand
	{
		public GetPositionMarkerParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetPositionMarkerParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

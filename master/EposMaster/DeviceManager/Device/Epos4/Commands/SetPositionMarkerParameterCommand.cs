using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetPositionMarkerParameterCommand : DeviceCommand
	{
		public SetPositionMarkerParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetPositionMarkerParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

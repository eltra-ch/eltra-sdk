using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivatePositionMarkerCommand : DeviceCommand
	{
		public DeactivatePositionMarkerCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DeactivatePositionMarker";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

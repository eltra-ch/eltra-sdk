using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivatePositionMarkerCommand : DeviceCommand
	{
		public ActivatePositionMarkerCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ActivatePositionMarker";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

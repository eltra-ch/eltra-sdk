using System;
using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivatePositionMarkerCommand : DeviceCommand
	{
		public DeactivatePositionMarkerCommand(EltraDevice device)
			:base(device)
		{
			Name = "DeactivatePositionMarker";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

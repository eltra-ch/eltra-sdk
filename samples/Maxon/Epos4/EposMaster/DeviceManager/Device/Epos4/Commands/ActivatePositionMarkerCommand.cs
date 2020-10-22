using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivatePositionMarkerCommand : DeviceCommand
	{
		public ActivatePositionMarkerCommand(EltraDevice device)
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

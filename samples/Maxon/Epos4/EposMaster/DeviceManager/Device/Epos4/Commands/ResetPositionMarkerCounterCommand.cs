using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ResetPositionMarkerCounterCommand : DeviceCommand
	{
		public ResetPositionMarkerCounterCommand(EltraDevice device)
			:base(device)
		{
			Name = "ResetPositionMarkerCounter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

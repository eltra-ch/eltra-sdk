using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadPositionMarkerCounterCommand : DeviceCommand
	{
		public ReadPositionMarkerCounterCommand(EltraDevice device)
			:base(device)
		{
			Name = "ReadPositionMarkerCounter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

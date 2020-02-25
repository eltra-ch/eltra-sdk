using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadPositionMarkerCounterCommand : DeviceCommand
	{
		public ReadPositionMarkerCounterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
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

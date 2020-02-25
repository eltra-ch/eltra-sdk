using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadPositionMarkerCapturedPositionCommand : DeviceCommand
	{
		public ReadPositionMarkerCapturedPositionCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ReadPositionMarkerCapturedPosition";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

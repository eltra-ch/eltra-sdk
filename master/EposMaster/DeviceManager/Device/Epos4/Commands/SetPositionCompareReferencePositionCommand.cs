using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetPositionCompareReferencePositionCommand : DeviceCommand
	{
		public SetPositionCompareReferencePositionCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetPositionCompareReferencePosition";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

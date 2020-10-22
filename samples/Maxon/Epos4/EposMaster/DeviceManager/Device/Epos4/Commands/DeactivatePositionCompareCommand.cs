using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DeactivatePositionCompareCommand : DeviceCommand
	{
		public DeactivatePositionCompareCommand(EltraDevice device)
			:base(device)
		{
			Name = "DeactivatePositionCompare";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

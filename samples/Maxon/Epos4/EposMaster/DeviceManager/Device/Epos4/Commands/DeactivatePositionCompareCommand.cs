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

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

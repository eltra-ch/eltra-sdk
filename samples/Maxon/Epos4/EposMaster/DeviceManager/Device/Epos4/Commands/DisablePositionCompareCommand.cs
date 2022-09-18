using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisablePositionCompareCommand : DeviceCommand
	{
		public DisablePositionCompareCommand(EltraDevice device)
			:base(device)
		{
			Name = "DisablePositionCompare";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

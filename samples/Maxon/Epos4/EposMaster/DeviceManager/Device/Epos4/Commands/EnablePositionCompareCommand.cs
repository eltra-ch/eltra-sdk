using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class EnablePositionCompareCommand : DeviceCommand
	{
		public EnablePositionCompareCommand(EltraDevice device)
			:base(device)
		{
			Name = "EnablePositionCompare";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

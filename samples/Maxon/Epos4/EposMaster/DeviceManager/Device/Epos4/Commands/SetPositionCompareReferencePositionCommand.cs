using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetPositionCompareReferencePositionCommand : DeviceCommand
	{
		public SetPositionCompareReferencePositionCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetPositionCompareReferencePosition";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

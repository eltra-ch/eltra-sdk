using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisablePositionWindowCommand : DeviceCommand
	{
		public DisablePositionWindowCommand(EltraDevice device)
			:base(device)
		{
			Name = "DisablePositionWindow";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisableVelocityWindowCommand : DeviceCommand
	{
		public DisableVelocityWindowCommand(EltraDevice device)
			:base(device)
		{
			Name = "DisableVelocityWindow";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

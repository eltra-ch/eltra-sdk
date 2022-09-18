using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ClearIpmBufferCommand : DeviceCommand
	{
		public ClearIpmBufferCommand(EltraDevice device)
			:base(device)
		{
			Name = "ClearIpmBuffer";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

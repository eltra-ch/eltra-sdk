using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetFreeIpmBufferSizeCommand : DeviceCommand
	{
		public GetFreeIpmBufferSizeCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetFreeIpmBufferSize";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadDataBufferCommand : DeviceCommand
	{
		public ReadDataBufferCommand(EltraDevice device)
			:base(device)
		{
			Name = "ReadDataBuffer";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

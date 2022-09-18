using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ExtractChannelDataVectorCommand : DeviceCommand
	{
		public ExtractChannelDataVectorCommand(EltraDevice device)
			:base(device)
		{
			Name = "ExtractChannelDataVector";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadChannelVectorSizeCommand : DeviceCommand
	{
		public ReadChannelVectorSizeCommand(EltraDevice device)
			:base(device)
		{
			Name = "ReadChannelVectorSize";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

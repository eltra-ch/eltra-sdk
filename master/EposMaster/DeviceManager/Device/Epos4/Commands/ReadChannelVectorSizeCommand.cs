using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadChannelVectorSizeCommand : DeviceCommand
	{
		public ReadChannelVectorSizeCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
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

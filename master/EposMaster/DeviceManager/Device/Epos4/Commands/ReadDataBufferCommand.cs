using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ReadDataBufferCommand : DeviceCommand
	{
		public ReadDataBufferCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ReadDataBuffer";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetIpmBufferParameterCommand : DeviceCommand
	{
		public SetIpmBufferParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetIpmBufferParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

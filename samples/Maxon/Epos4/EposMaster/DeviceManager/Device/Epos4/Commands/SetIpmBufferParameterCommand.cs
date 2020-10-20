using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetIpmBufferParameterCommand : DeviceCommand
	{
		public SetIpmBufferParameterCommand(EltraDevice device)
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

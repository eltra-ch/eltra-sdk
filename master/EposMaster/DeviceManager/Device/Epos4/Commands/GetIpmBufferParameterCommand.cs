using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetIpmBufferParameterCommand : DeviceCommand
	{
		public GetIpmBufferParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetIpmBufferParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

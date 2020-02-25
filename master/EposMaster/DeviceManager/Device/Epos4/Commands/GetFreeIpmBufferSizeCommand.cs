using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetFreeIpmBufferSizeCommand : DeviceCommand
	{
		public GetFreeIpmBufferSizeCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetFreeIpmBufferSize";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

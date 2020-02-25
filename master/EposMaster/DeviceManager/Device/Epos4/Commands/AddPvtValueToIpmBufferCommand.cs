using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class AddPvtValueToIpmBufferCommand : DeviceCommand
	{
		public AddPvtValueToIpmBufferCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "AddPvtValueToIpmBuffer";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

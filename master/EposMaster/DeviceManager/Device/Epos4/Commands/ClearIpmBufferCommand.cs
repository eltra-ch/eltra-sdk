using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ClearIpmBufferCommand : DeviceCommand
	{
		public ClearIpmBufferCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ClearIpmBuffer";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

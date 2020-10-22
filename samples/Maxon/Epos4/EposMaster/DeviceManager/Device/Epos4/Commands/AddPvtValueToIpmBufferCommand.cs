using System;
using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class AddPvtValueToIpmBufferCommand : DeviceCommand
	{
		public AddPvtValueToIpmBufferCommand(EltraDevice device)
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

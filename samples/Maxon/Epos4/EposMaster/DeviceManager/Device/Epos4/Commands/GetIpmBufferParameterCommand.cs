using System;
using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetIpmBufferParameterCommand : DeviceCommand
	{
		public GetIpmBufferParameterCommand(EltraDevice device)
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

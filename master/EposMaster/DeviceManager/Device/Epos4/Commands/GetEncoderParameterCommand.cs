using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetEncoderParameterCommand : DeviceCommand
	{
		public GetEncoderParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetEncoderParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

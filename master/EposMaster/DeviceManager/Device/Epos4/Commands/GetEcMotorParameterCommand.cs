using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetEcMotorParameterCommand : DeviceCommand
	{
		public GetEcMotorParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetEcMotorParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetDcMotorParameterCommand : DeviceCommand
	{
		public GetDcMotorParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetDcMotorParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

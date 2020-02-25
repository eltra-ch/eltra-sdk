using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetHallSensorParameterCommand : DeviceCommand
	{
		public GetHallSensorParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetHallSensorParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

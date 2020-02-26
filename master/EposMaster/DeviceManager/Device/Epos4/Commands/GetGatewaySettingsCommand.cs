using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetGatewaySettingsCommand : DeviceCommand
	{
		public GetGatewaySettingsCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetGatewaySettings";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
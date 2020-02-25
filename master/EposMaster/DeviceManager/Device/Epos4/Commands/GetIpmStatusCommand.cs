using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetIpmStatusCommand : DeviceCommand
	{
		public GetIpmStatusCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetIpmStatus";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

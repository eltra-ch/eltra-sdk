using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetDriverInfoCommand : DeviceCommand
	{
		public GetDriverInfoCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetDriverInfo";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

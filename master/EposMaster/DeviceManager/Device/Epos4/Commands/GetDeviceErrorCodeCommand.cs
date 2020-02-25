using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetDeviceErrorCodeCommand : DeviceCommand
	{
		public GetDeviceErrorCodeCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetDeviceErrorCode";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

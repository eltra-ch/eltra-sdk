using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetDeviceNameCommand : DeviceCommand
	{
		public GetDeviceNameCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetDeviceName";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

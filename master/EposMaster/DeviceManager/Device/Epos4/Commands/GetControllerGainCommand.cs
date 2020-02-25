using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetControllerGainCommand : DeviceCommand
	{
		public GetControllerGainCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetControllerGain";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivateMasterEncoderModeCommand : DeviceCommand
	{
		public ActivateMasterEncoderModeCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ActivateMasterEncoderMode";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

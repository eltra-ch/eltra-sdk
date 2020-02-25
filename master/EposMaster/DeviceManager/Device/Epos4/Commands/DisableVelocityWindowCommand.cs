using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisableVelocityWindowCommand : DeviceCommand
	{
		public DisableVelocityWindowCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "DisableVelocityWindow";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

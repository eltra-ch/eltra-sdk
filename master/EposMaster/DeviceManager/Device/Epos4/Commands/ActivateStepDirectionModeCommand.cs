using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivateStepDirectionModeCommand : DeviceCommand
	{
		public ActivateStepDirectionModeCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ActivateStepDirectionMode";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetCurrentRegulatorGainCommand : DeviceCommand
	{
		public GetCurrentRegulatorGainCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetCurrentRegulatorGain";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

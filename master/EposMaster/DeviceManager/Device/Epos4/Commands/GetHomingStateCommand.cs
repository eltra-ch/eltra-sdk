using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetHomingStateCommand : DeviceCommand
	{
		public GetHomingStateCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetHomingState";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

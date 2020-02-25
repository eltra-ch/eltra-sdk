using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ExtractChannelDataVectorCommand : DeviceCommand
	{
		public ExtractChannelDataVectorCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ExtractChannelDataVector";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

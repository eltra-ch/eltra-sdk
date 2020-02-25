using System;
using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ExportParameterCommand : DeviceCommand
	{
		public ExportParameterCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ExportParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

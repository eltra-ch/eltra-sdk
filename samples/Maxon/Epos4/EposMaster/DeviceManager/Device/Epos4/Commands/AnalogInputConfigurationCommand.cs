using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class AnalogInputConfigurationCommand : DeviceCommand
	{
		public AnalogInputConfigurationCommand(EltraDevice device)
			:base(device)
		{
			Name = "AnalogInputConfiguration";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

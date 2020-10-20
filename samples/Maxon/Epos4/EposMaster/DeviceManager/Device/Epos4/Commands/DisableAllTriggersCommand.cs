using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class DisableAllTriggersCommand : DeviceCommand
	{
		public DisableAllTriggersCommand(EltraDevice device)
			:base(device)
		{
			Name = "DisableAllTriggers";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ForceTriggerCommand : DeviceCommand
	{
		public ForceTriggerCommand(EltraDevice device)
			:base(device)
		{
			Name = "ForceTrigger";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

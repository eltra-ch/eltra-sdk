using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetControllerGainCommand : DeviceCommand
	{
		public SetControllerGainCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetControllerGain";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

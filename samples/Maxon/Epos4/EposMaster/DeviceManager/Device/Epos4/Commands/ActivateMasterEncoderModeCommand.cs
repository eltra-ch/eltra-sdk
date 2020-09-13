using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivateMasterEncoderModeCommand : DeviceCommand
	{
		public ActivateMasterEncoderModeCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivateMasterEncoderMode";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

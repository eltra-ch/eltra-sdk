using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivateInterpolatedPositionModeCommand : DeviceCommand
	{
		public ActivateInterpolatedPositionModeCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivateInterpolatedPositionMode";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

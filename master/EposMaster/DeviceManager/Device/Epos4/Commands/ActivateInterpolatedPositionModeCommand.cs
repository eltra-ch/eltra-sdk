using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ActivateInterpolatedPositionModeCommand : DeviceCommand
	{
		public ActivateInterpolatedPositionModeCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
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

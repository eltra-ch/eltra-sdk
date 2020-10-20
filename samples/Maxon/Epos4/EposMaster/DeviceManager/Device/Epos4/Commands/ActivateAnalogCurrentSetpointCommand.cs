using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
	class ActivateAnalogCurrentSetpointCommand : DeviceCommand
	{
		public ActivateAnalogCurrentSetpointCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivateAnalogCurrentSetpoint";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}

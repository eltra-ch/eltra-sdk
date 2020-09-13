using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class EnableTriggerCommand : SetTriggerConfigurationCommand
	{
        public EnableTriggerCommand()
        { }
		
        public EnableTriggerCommand(EltraDevice device)
			:base(device)
		{
			Name = "EnableTrigger";
        }

        public override DeviceCommand Clone()
        {
            if (Clone(out EnableTriggerCommand result))
            {
                result.Device = Device;
            }

            return result;
        }
	}
}

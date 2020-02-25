using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class EnableTriggerCommand : SetTriggerConfigurationCommand
	{
        public EnableTriggerCommand()
        { }
		
        public EnableTriggerCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
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

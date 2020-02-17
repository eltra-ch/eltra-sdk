using EltraCloudContracts.Contracts.ToolSet;

namespace ThermoMaster.DeviceManager.Device.Thermostat.Tools
{
    public class ThermoControlTool : DeviceTool
    {
        #region Constructors
        
        public ThermoControlTool()
        {
            Uuid = "6973CFB4-B798-4204-9C66-569CD0CB6BDA";
            Name = "Thermo Control";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

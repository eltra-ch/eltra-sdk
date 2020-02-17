using EltraCloudContracts.Contracts.ToolSet;

namespace ThermoMaster.DeviceManager.Device.Thermostat.Tools
{
    class ThermoHistoryTool : DeviceTool
    {
        #region Constructors

        public ThermoHistoryTool()
        {
            Uuid = "FBC7F717-5F88-449D-B13D-A6CB866863E9";
            Name = "Thermo History";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

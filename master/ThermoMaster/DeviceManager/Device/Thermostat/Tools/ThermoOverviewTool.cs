using EltraCloudContracts.Contracts.ToolSet;

namespace ThermoMaster.DeviceManager.Device.Thermostat.Tools
{
    class ThermoOverviewTool : DeviceTool
    {
        #region Constructors

        public ThermoOverviewTool()
        {
            Uuid = "D0CD9514-0796-4795-A730-6D936155DE76";
            Name = "Thermo Overview";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

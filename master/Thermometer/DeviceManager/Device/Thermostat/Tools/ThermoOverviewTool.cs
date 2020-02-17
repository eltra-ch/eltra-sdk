using EltraCloudContracts.Contracts.ToolSet;

namespace ThermoMaster.DeviceManager.Device.Thermostat.Tools
{
    class ThermoOverviewTool : DeviceTool
    {
        #region Constructors

        public ThermoOverviewTool()
        {
            Uuid = "C9CCCCF8-ADE4-4396-B65E-731D294910B0";
            Name = "Thermometer Overview";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

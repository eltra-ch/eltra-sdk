using EltraCloudContracts.Contracts.ToolSet;

namespace ThermoMaster.DeviceManager.Device.Thermostat.Tools
{
    class ThermoSettingsTool : DeviceTool
    {
        #region Constructors

        public ThermoSettingsTool()
        {
            Uuid = "2CB6FB03-2428-4E2E-84EF-58A2633233AE";
            Name = "Thermometer Settings";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

using EltraCloudContracts.Contracts.ToolSet;

namespace ThermoMaster.DeviceManager.Device.Thermostat.Tools
{
    class ThermoSettingsTool : DeviceTool
    {
        #region Constructors

        public ThermoSettingsTool()
        {
            Uuid = "14CB39F2-F2BE-46CC-AEB6-72201B2021A0";
            Name = "Thermo Settings";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

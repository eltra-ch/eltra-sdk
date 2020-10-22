using EltraCommon.Contracts.ToolSet;

namespace EposMaster.DeviceManager.Device.Epos4.Tools
{
    public class Epos4HomingModeTool : DeviceTool
    {
        #region Constructors

        public Epos4HomingModeTool()
        {
            Id = "A6EAAAD9-0B2B-4A2A-A402-E7105A72C5E0";
            Name = "Homing";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

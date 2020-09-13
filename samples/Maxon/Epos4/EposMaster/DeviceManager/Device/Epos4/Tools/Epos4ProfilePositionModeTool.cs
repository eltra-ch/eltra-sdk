using EltraCommon.Contracts.ToolSet;

namespace EposMaster.DeviceManager.Device.Epos4.Tools
{
    public class Epos4ProfilePositionModeTool : DeviceTool
    {
        #region Constructors

        public Epos4ProfilePositionModeTool()
        {
            Id = "18AC4A72-831F-4EB4-9400-C50569D3919A";
            Name = "Profile Position Mode";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

using EltraCommon.Contracts.ToolSet;

namespace EposMaster.DeviceManager.Device.Epos4.Tools
{
    public class Epos4ProfileVelocityModeTool : DeviceTool
    {
        #region Constructors

        public Epos4ProfileVelocityModeTool()
        {
            Id = "13AFB2FD-3238-475B-9F80-27C52A638908";
            Name = "Profile Velocity Mode";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

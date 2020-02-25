using EltraCloudContracts.Contracts.ToolSet;

namespace EposMaster.DeviceManager.Device.Epos4.Tools
{
    public class Epos4DataRecorderTool : DeviceTool
    {
        #region Constructors

        public Epos4DataRecorderTool()
        {
            Uuid = "1440B525-0DE2-46C7-872F-CE4FAE1B02DC";
            Name = "Data Recorder";
            Status = DeviceToolStatus.Disabled;
        }

        #endregion
    }
}

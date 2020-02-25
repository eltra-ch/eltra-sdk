using EltraCloudContracts.Contracts.ToolSet;

namespace EposMaster.DeviceManager.Device.Epos4.Tools
{
    public class Epos4ObjectDictionaryTool : DeviceTool
    {
        #region Constructors
        
        public Epos4ObjectDictionaryTool()
        {
            Uuid = "B47D8049-914B-4984-A883-90CF537F0318";
            Name = "Object Dictionary";
            Status = DeviceToolStatus.Enabled;
        }

        #endregion
    }
}

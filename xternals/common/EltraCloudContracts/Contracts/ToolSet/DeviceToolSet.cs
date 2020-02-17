using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.ToolSet
{
    [DataContract]
    public class DeviceToolSet
    {
        #region Private fields

        private List<DeviceTool> _tools;

        #endregion

        #region Properties

        [DataMember]
        public List<DeviceTool> Tools => _tools ?? (_tools = new List<DeviceTool>());

        #endregion

        #region Methods

        public bool AddTool(DeviceTool tool)
        {
            bool result = false;

            if (!ToolExists(tool))
            {
                Tools.Add(tool);
                result = true;
            }

            return result;
        }

        public bool ToolExists(DeviceTool tool)
        {
            return FindToolByUuid(tool.Uuid) != null;
        }

        public DeviceTool FindToolByUuid(string uuid)
        {
            DeviceTool result = null;

            if(!string.IsNullOrEmpty(uuid))
            {
                foreach (var command in Tools)
                {
                    if (command.Uuid.ToLower() == uuid.ToLower())
                    {
                        result = command;
                        break;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}

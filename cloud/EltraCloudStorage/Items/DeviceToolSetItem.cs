using System;
using EltraCommon.Logger;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;
using EltraCloudContracts.Contracts.ToolSet;

namespace EltraCloudStorage.Items
{
    class DeviceToolSetItem : StorageItem
    {
        #region Constructors

        public DeviceToolSetItem()
        {           
        }

        #endregion

        #region Methods

        public bool AddToolSet(DeviceToolSet deviceToolSet, int deviceId)
        {
            bool result = true;

            foreach (var tool in deviceToolSet.Tools)
            {
                if (GetToolId(tool.Uuid, out var toolId) && toolId > 0)
                {
                    result = UpdateTool(toolId, tool);
                }
                else
                {
                    result = AddTool(tool);

                    if (result)
                    {
                        result = GetToolId(tool.Uuid, out toolId) && toolId > 0;
                    }
                }

                if (result)
                {
                    result = AddToolSet(deviceId, toolId);
                }

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        public bool UpdateToolSet(int deviceId, DeviceToolSet deviceToolSet)
        {
            bool result = true;

            foreach (var tool in deviceToolSet.Tools)
            {
                result = !GetToolId(tool.Uuid, out var toolId) ? AddTool(tool) : UpdateTool(toolId, tool);

                if (result)
                {
                    result = UpdateToolSet(deviceId, toolId);
                }

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        private bool AddToolSet(int deviceId, int toolId)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertDeviceToolSet));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@device_id", deviceId));
                    command.Parameters.Add(new DbParameterWrapper("@tool_id", toolId));
                    
                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("DeviceToolSetItem - AddToolSet", e);

                result = false;
            }

            return result;
        }

        public bool GetToolId(string uuid, out int toolId)
        {
            bool result = false;

            toolId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectToolByUuid));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        toolId = Convert.ToInt32(queryResult);
                        result = toolId > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("DeviceToolSetItem - GetToolId", e);
            }

            return result;
        }

        private bool AddTool(DeviceTool deviceTool)
        {
            bool result = true;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertTool));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", deviceTool.Uuid));
                    command.Parameters.Add(new DbParameterWrapper("@name", deviceTool.Name));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)deviceTool.Status));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("DeviceToolSetItem - AddTool", e);

                result = false;
            }
            
            return result;
        }
        
        private bool UpdateToolSet(int deviceId, int toolId)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateToolSet));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@device_id", deviceId));
                    command.Parameters.Add(new DbParameterWrapper("@tool_id", toolId));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("DeviceToolSetItem - UpdateToolSet", e);

                result = false;
            }

            return result;
        }
        
        private bool UpdateTool(int toolId, DeviceTool tool)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateTool));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@tool_id", toolId));                    
                    command.Parameters.Add(new DbParameterWrapper("@status", tool.Status));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("DeviceToolSetItem - UpdateTool", e);

                result = false;
            }

            return result;
        }
    }
     
    #endregion
}

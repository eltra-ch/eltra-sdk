using System;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;

namespace EltraCloudStorage.Items
{
    class DeviceCommandSetItem : StorageItem
    {
        public DeviceCommandSetItem()
        {
            DataTypeStorageItem = new DataTypeStorageItem {Engine = Engine};

            AddChild(DataTypeStorageItem);
        }

        private DataTypeStorageItem DataTypeStorageItem { get; }


        public bool AddCommandSet(DeviceCommandSet deviceCommandSet, int sessionId, int deviceId)
        {
            bool result = true;

            foreach (var command in deviceCommandSet.Commands)
            {
                if (GetCommandId(deviceId, command.Name, out var commandId) && commandId > 0)
                {
                    result = UpdateCommand(command, commandId);
                }
                else
                {
                    result = AddCommand(command, deviceId);

                    if (result)
                    {
                        result = GetCommandId(deviceId, command.Name, out commandId) && commandId > 0;
                    }
                }

                if (result)
                {
                    result = AddSessionCommand(sessionId, commandId, command.Status);
                }

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        private bool AddSessionCommand(int sessionId, int commandId, ExecCommandStatus status)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertSessionCommand));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@session_id", sessionId));
                    command.Parameters.Add(new DbParameterWrapper("@command_id", commandId));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddSessionCommand", e);

                result = false;
            }

            return result;
        }

        public bool GetCommandId(int deviceId, string name, out int commandId)
        {
            bool result = false;

            commandId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectCommand));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@device_id", deviceId));
                    command.Parameters.Add(new DbParameterWrapper("@name", name));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        commandId = Convert.ToInt32(queryResult);
                        result = commandId > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetCommandId", e);
            }

            return result;
        }

        private bool AddCommand(DeviceCommand deviceCommand, int deviceId)
        {
            bool result = true;

            try
            {
                var name = deviceCommand.Name;
                
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertCommand));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@device_id", deviceId));
                    command.Parameters.Add(new DbParameterWrapper("@name", name));
                    
                    if (command.ExecuteNonQuery() > 0)
                    {
                        if (GetCommandId(deviceId, name, out var commandId))
                        {
                            result = AddParameters(deviceCommand, commandId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddCommand", e);

                result = false;
            }
            
            return result;
        }

        private bool AddParameters(DeviceCommand command, int commandId)
        {
            bool result = true;

            foreach (var parameter in command.Parameters)
            {
                result = AddParameter(parameter, commandId);

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        private bool AddParameter(DeviceCommandParameter parameter, int commandId)
        {
            bool result = false;
            var name = parameter.Name;
            var type = parameter.Type;
            var dataType = parameter.DataType;

            if (DataTypeStorageItem.SetGetDataTypeId(dataType, out int dataTypeId))
            {
                try
                {
                    string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertCommandParameter));

                    using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                    {
                        command.Parameters.Add(new DbParameterWrapper("@command_id", commandId));
                        command.Parameters.Add(new DbParameterWrapper("@type", (int)type));
                        command.Parameters.Add(new DbParameterWrapper("@name", name));
                        command.Parameters.Add(new DbParameterWrapper("@data_type_id", dataTypeId));
                        
                        if (command.ExecuteNonQuery() > 0)
                        {
                            result = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - AddParameter", e);

                    result = false;
                }
            }

            return result;
        }

        public bool UpdateCommandSet(DeviceCommandSet deviceCommandSet, int sessionId, int deviceId)
        {
            bool result = true;

            foreach (var command in deviceCommandSet.Commands)
            {
                result = !GetCommandId(deviceId, command.Name, out var commandId) ? AddCommand(command, deviceId) : UpdateCommand(command, commandId);

                if (result)
                {
                    result = UpdateSessionCommand(sessionId, commandId, command);
                }

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        private bool UpdateSessionCommand(int sessionId, int commandId, DeviceCommand deviceCommand)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateSessionCommand));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@session_id", sessionId));
                    command.Parameters.Add(new DbParameterWrapper("@command_id", commandId));
                    command.Parameters.Add(new DbParameterWrapper("@status", deviceCommand.Status));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UpdateSessionCommand", e);

                result = false;
            }

            return result;
        }
        
        private bool UpdateCommand(DeviceCommand command, int commandId)
        {
            bool result = true;

            foreach (var parameter in command.Parameters)
            {
                result = DataTypeStorageItem.SetGetDataTypeId(parameter.DataType, out var dataTypeId);

                if (result)
                {
                    if (!GetParameterId(dataTypeId, commandId, parameter.Name, out var _))
                    {
                        result = AddParameter(parameter, commandId);
                    }
                }

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        public bool GetParameterId(DeviceCommandParameter parameter, int commandId, out int parameterId)
        {
            bool result = DataTypeStorageItem.SetGetDataTypeId(parameter.DataType, out var dataTypeId);

            parameterId = 0;

            if (result)
            {
                result = GetParameterId(dataTypeId, commandId, parameter.Name, out parameterId);

                if (!result || parameterId == 0)
                {
                    if (AddParameter(parameter, commandId))
                    {
                        result = GetParameterId(dataTypeId, commandId, parameter.Name, out parameterId);
                    }
                }
            }

            return result;
        }

        public bool GetParameterId(int dataTypeId, int commandId, string name, out int parameterId)
        {
            bool result = false;

            parameterId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectCommandParameter));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@data_type_id", dataTypeId));
                    command.Parameters.Add(new DbParameterWrapper("@command_id", commandId));
                    command.Parameters.Add(new DbParameterWrapper("@name", name));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        parameterId = Convert.ToInt32(queryResult);

                        result = parameterId > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetParameterId", e);
            }

            return result;
        }

        public ExecuteCommandStatus GetCommandStatus(string uuid, string sessionUuid, ulong serialNumber, string commandName)
        {
            ExecuteCommandStatus result = null;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectCommandStatus));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));
                    command.Parameters.Add(new DbParameterWrapper("@session_uuid", sessionUuid));
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@command_name", commandName));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new ExecuteCommandStatus
                            {
                                CommandUuid = uuid,
                                SerialNumber = serialNumber,
                                CommandName = commandName,
                                SessionUuid = reader.GetString(0),
                                Status = (ExecCommandStatus) reader.GetInt32(1),
                                Modified = reader.GetDateTime(2)
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetCommandStatus", e);
            }

            return result;
        }
    }
}

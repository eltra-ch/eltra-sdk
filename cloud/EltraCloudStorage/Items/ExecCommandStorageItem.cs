using System;
using System.Collections.Generic;
using EltraCloudStorage.Items;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;

namespace EltraCloudStorage.Items
{
    class ExecCommandStorageItem : StorageItem
    {
        #region Properties

        public DeviceCommandSetItem DeviceCommandSet { get; set; }

        #endregion

        #region Methods

        #region Interface 

        public bool Update(string commandUuid, int sessionUuid, int commandId, DeviceCommand deviceCommand)
        {
            bool result = false;
            var start = MsgLogger.BeginTimeMeasure();
            var updateTransaction = Connection.BeginTransaction();

            if (updateTransaction != null)
            {
                if (GetExecCommandId(commandUuid, sessionUuid, commandId, out var execCommandId) && execCommandId > 0)
                {
                    result = UpdateExecCommand(execCommandId, commandId, deviceCommand);
                }
                else
                {
                    result = AddExecCommand(commandUuid, sessionUuid, commandId, deviceCommand);
                }

                if (result)
                {
                    updateTransaction.Commit();
                }
                else
                {
                    updateTransaction.Rollback();
                }
            }

            MsgLogger.EndTimeMeasure("ExecCommandStorageItem - Update", start, "Exec command - add/update command");

            return result;
        }

        public bool UpdateStatus(string commandUuid, int sessionUuid, int commandId, ExecCommandStatus status)
        {
            bool result = false;

            var updateTransaction = Connection.BeginTransaction();

            if (updateTransaction != null)
            {
                if (GetExecCommandId(commandUuid, sessionUuid, commandId, out var execCommandId) && execCommandId > 0)
                {
                    result = UpdateExecCommandStatus(execCommandId, status);
                }

                if (result)
                {
                    updateTransaction.Commit();
                }
                else
                {
                    updateTransaction.Rollback();
                }
            }

            return result;
        }

        public bool UpdateCommStatus(string commandUuid, int sessionUuid, int commandId, ExecCommandCommStatus status)
        {
            bool result = false;

            var updateTransaction = Connection.BeginTransaction();

            if (updateTransaction != null)
            {
                if (GetExecCommandId(commandUuid, sessionUuid, commandId, out var execCommandId) && execCommandId > 0)
                {
                    result = UpdateExecCommandCommStatus(execCommandId, status);
                }

                if (result)
                {
                    updateTransaction.Commit();
                }
                else
                {
                    updateTransaction.Rollback();
                }
            }

            return result;
        }

        public bool GetExecCommandId(string uuid, int sessionUuid, int commandId, out int execCommandId)
        {
            bool result = false;

            execCommandId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectExecCommandId));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));
                    command.Parameters.Add(new DbParameterWrapper("@command_id", commandId));
                    command.Parameters.Add(new DbParameterWrapper("@session_id", sessionUuid));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        execCommandId = Convert.ToInt32(queryResult);
                        result = execCommandId > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - GetExecCommandId", e);
            }

            return result;
        }

        public List<ExecuteCommand> PopCommands(ulong serialNumber, ExecCommandStatus status)
        {
            var result = new List<ExecuteCommand>();
            const int maxCommandDelayInSec = 30;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectPopCommands));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serialNumber", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));
                    command.Parameters.Add(new DbParameterWrapper("@session_status", (int)SessionStatus.Online));
                    command.Parameters.Add(new DbParameterWrapper("@max_delay", maxCommandDelayInSec));
                    
                    var dbExecCommands = new List<DbExecCommands>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var execCommand = new ExecuteCommand();

                            var commandUuid = reader.GetString(0);

                            int execCommandId = reader.GetInt32(1);
                            var commandName = reader.GetString(2);
                            string uuid = reader.GetString(3);
                            DateTime modified = reader.GetDateTime(4);

                            execCommand.CommandUuid = commandUuid;
                            execCommand.SessionUuid = uuid;
                            execCommand.SerialNumber = serialNumber;
                            execCommand.Modified = modified;

                            dbExecCommands.Add(new DbExecCommands { Command = execCommand, CommandId = execCommandId, CommandName = commandName });
                        }
                    }

                    foreach (var dbExecCommand in dbExecCommands)
                    {
                        var execCommand = dbExecCommand.Command;

                        execCommand.Command = GetDeviceCommandParameters(dbExecCommand.CommandId, dbExecCommand.CommandName);    

                        result.Add(execCommand);
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - PopCommands", e);
            }

            return result;
        }

        public List<ExecuteCommand> PopCommands(ulong[] serialNumberArray, ExecCommandStatus[] statusArray)
        {
            var result = new List<ExecuteCommand>();
            const int maxCommandDelayInSec = 30;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectMultiPopCommands));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    int[] intStatusArray = Array.ConvertAll(statusArray, value => (int)value);

                    command.Parameters.Add(new DbParameterWrapper("@serialNumberArray", string.Join(",", serialNumberArray)));
                    command.Parameters.Add(new DbParameterWrapper("@statusArray", string.Join(",", intStatusArray)));
                    command.Parameters.Add(new DbParameterWrapper("@session_status", (int)SessionStatus.Online));
                    command.Parameters.Add(new DbParameterWrapper("@communication_status", (int)ExecCommandCommStatus.Undefined));
                    command.Parameters.Add(new DbParameterWrapper("@max_delay", maxCommandDelayInSec));

                    var dbExecCommands = new List<DbExecCommands>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader != null && reader.Read())
                        {
                            var execCommand = new ExecuteCommand();

                            ulong serialNumber = reader.GetUInt64(0);
                            var commandUuid = reader.GetString(1);
                            int execCommandId = reader.GetInt32(2);
                            var commandName = reader.GetString(3);
                            string uuid = reader.GetString(4);
                            DateTime modified = reader.GetDateTime(5);

                            execCommand.CommandUuid = commandUuid;
                            execCommand.SessionUuid = uuid;
                            execCommand.SerialNumber = serialNumber;
                            execCommand.Modified = modified;

                            dbExecCommands.Add(new DbExecCommands { Command = execCommand, CommandId = execCommandId, CommandName = commandName });
                        }
                    }

                    foreach (var dbExecCommand in dbExecCommands)
                    {
                        var execCommand = dbExecCommand.Command;

                        execCommand.Command = GetDeviceCommandParameters(dbExecCommand.CommandId, dbExecCommand.CommandName);

                        result.Add(execCommand);
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - PopCommands", e);
            }

            return result;
        }

        public List<ExecuteCommand> GetExecCommands(List<string> commandUuids)
        {
            throw new NotImplementedException();
        }

        public List<ExecuteCommand> GetExecCommands(string sessionUuid, ExecCommandStatus[] statusArray)
        {
            var result = new List<ExecuteCommand>();
            const int maxCommandDelayInSec = 30;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectExecCommands));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    int[] intStatusArray = Array.ConvertAll(statusArray, value => (int)value);

                    command.Parameters.Add(new DbParameterWrapper("@source_session_uuid", sessionUuid));
                    command.Parameters.Add(new DbParameterWrapper("@statusArray", string.Join(",", intStatusArray)));
                    command.Parameters.Add(new DbParameterWrapper("@session_status", (int)SessionStatus.Online));
                    command.Parameters.Add(new DbParameterWrapper("@communication_status", (int)ExecCommandCommStatus.SentToMaster));
                    command.Parameters.Add(new DbParameterWrapper("@max_delay", maxCommandDelayInSec));

                    var dbExecCommands = new List<DbExecCommands>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader != null && reader.Read())
                        {
                            var execCommand = new ExecuteCommand();

                            ulong serialNumber = reader.GetUInt64(0);
                            var commandUuid = reader.GetString(1);
                            int execCommandId = reader.GetInt32(2);
                            var commandName = reader.GetString(3);
                            string uuid = reader.GetString(4);
                            var execCommandStatus = (ExecCommandStatus)reader.GetInt32(5);
                            DateTime modified = reader.GetDateTime(6);

                            execCommand.CommandUuid = commandUuid;
                            execCommand.SessionUuid = uuid;
                            execCommand.SerialNumber = serialNumber;
                            execCommand.Modified = modified;

                            dbExecCommands.Add(new DbExecCommands { 
                                Command = execCommand, CommandId = execCommandId, 
                                CommandName = commandName, Status = execCommandStatus });
                        }
                    }

                    foreach (var dbExecCommand in dbExecCommands)
                    {
                        var execCommand = dbExecCommand.Command;

                        execCommand.Command = GetDeviceCommandParameters(dbExecCommand.CommandId, dbExecCommand.CommandName);
                        
                        if(execCommand.Command!=null)
                        { 
                            execCommand.Command.Status = dbExecCommand.Status;
                            execCommand.Command.Uuid = execCommand.CommandUuid;
                        }

                        result.Add(execCommand);
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - GetExecCommands", e);
            }

            return result;
        }

        public ExecuteCommand PopCommand(string commandUuid, ulong serialNumber, ExecCommandStatus status)
        {
            ExecuteCommand result = null;
            const int maxCommandDelayInSec = 30;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSpecificPopCommand));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", commandUuid));
                    command.Parameters.Add(new DbParameterWrapper("@serialNumber", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));
                    command.Parameters.Add(new DbParameterWrapper("@session_status", (int)SessionStatus.Online));
                    command.Parameters.Add(new DbParameterWrapper("@max_delay", maxCommandDelayInSec));

                    int execCommandId = 0;
                    string commandName = string.Empty;
                    
                    ExecuteCommand execCommand = null;

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            execCommand = new ExecuteCommand { CommandUuid = commandUuid };
                            
                            execCommandId = reader.GetInt32(0);
                            commandName = reader.GetString(1);
                            var uuid = reader.GetString(2);
                            var modified = reader.GetDateTime(3);

                            execCommand.SessionUuid = uuid;
                            execCommand.SerialNumber = serialNumber;
                            execCommand.Modified = modified;
                        }
                    }

                    if(execCommand!=null)
                    {
                        execCommand.Command = GetDeviceCommandParameters(execCommandId, commandName);
                    }

                    result = execCommand;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - PopCommand", e);
            }

            return result;
        }

        #endregion Interface

        #region Private

        private bool AddExecCommand(string uuid, int sessionUuid, int commandId, ExecCommandStatus status)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertExecCommand));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));
                    command.Parameters.Add(new DbParameterWrapper("@session_id", sessionUuid));
                    command.Parameters.Add(new DbParameterWrapper("@command_id", commandId));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                    int queryResult = command.ExecuteNonQuery();

                    if (queryResult > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - AddExecCommand", e);
            }

            return result;
        }

        private bool AddExecCommand(string commandUuid, int sessionUuid, int commandId, DeviceCommand deviceCommand)
        {
            bool result = AddExecCommand(commandUuid, sessionUuid, commandId, ExecCommandStatus.Announced);

            if (result && GetExecCommandId(commandUuid, sessionUuid, commandId, out var execCommandId))
            {
                foreach (var parameter in deviceCommand.Parameters)
                {
                    result = DeviceCommandSet.GetParameterId(parameter, commandId, out var commandParameterId);

                    if (result)
                    {
                        result = AddExecCommandParameter(execCommandId, commandParameterId, parameter.Value);
                    }

                    if(!result)
                    {
                        break;
                    }
                }

                if (result)
                {
                    result = UpdateExecCommandStatus(execCommandId, deviceCommand.Status);
                }
            }
            
            return result;
        }

        private bool AddExecCommandParameter(int execCommandId, int commandParameterId, string parameterValue)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertExecCommandParameter));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@exec_command_id", execCommandId));
                    command.Parameters.Add(new DbParameterWrapper("@command_parameter_id", commandParameterId));
                    command.Parameters.Add(new DbParameterWrapper("@value", parameterValue));

                    int queryResult = command.ExecuteNonQuery();

                    if (queryResult > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - AddExecCommandParameter", e);
            }

            return result;
        }

        private bool UpdateExecCommand(int execCommandId, int commandId, DeviceCommand deviceCommand)
        {
            bool result = false;

            foreach (var parameter in deviceCommand.Parameters)
            {
                result = DeviceCommandSet.GetParameterId(parameter, commandId, out var commandParameterId);

                if (result)
                {
                    GetExecCommandParameterId(execCommandId, commandParameterId, out var execCommandParameterId);

                    if (execCommandParameterId > 0)
                    {
                        result = UpdateExecCommandParameterValue(execCommandId, commandParameterId, parameter);
                    }
                    else
                    {
                        result = AddExecCommandParameter(execCommandId, commandParameterId, parameter.Value);
                    }
                }

                if (!result)
                {
                    break;
                }
            }

            if (result)
            {
                result = UpdateExecCommandStatus(execCommandId, deviceCommand.Status);
            }

            return result;
        }

        private bool GetExecCommandParameterId(int execCommandId, int commandParameterId, out int execCommandParameterId)
        {
            bool result = false;
            
            execCommandParameterId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectExecCommandParameter));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@exec_command_id", execCommandId));
                    command.Parameters.Add(new DbParameterWrapper("@command_parameter_id", commandParameterId));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        execCommandParameterId = Convert.ToInt32(queryResult);
                        result = execCommandId > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - GetExecCommandParameterId", e);
            }

            return result;
        }
        
        private bool UpdateExecCommandParameterValue(int execCommandId, int commandParameterId, DeviceCommandParameter deviceParameter)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateExecCommandParameterValue));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@exec_command_id", execCommandId));
                    command.Parameters.Add(new DbParameterWrapper("@command_parameter_id", commandParameterId));
                    command.Parameters.Add(new DbParameterWrapper("@value", deviceParameter.Value));

                    int queryResult = command.ExecuteNonQuery();

                    if (queryResult > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - UpdateExecCommandParameterValue", e);
            }

            return result;
        }

        private bool UpdateExecCommandStatus(int execCommandId, ExecCommandStatus status)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateExecCommandStatus));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@exec_command_id", execCommandId));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                    int queryResult = command.ExecuteNonQuery();

                    if (queryResult > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - UpdateExecCommandStatus", e);
            }

            return result;
        }

        private bool UpdateExecCommandCommStatus(int execCommandId, ExecCommandCommStatus status)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateExecCommandCommStatus));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@exec_command_id", execCommandId));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                    int queryResult = command.ExecuteNonQuery();

                    if (queryResult > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - UpdateExecCommandCommStatus", e);
            }

            return result;
        }

        private DeviceCommand GetDeviceCommandParameters(int execCommandId, string commandName)
        {
            DeviceCommand result = null;

            try
            {
                var deviceCommand = new DeviceCommand { Name = commandName, Status = ExecCommandStatus.Waiting };

                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectExecParameter));
                
                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@exec_command_id", execCommandId));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var parameter = new DeviceCommandParameter();

                            var parameterName = reader.GetString(0);
                            int type = reader.GetInt32(1);
                            int dataType = reader.GetInt32(2);
                            uint sizeBytes = (uint) reader.GetInt32(3);
                            uint sizeBits = (uint) reader.GetInt32(4);
                            string value = string.Empty;

                            if (!reader.IsDBNull(5))
                            {
                                value = reader.GetString(5);
                            }

                            parameter.Name = parameterName;
                            parameter.Value = value;
                            parameter.Type = (ParameterType) type;
                            parameter.DataType = new DataType
                                {Type = (TypeCode) dataType, SizeInBytes = sizeBytes, SizeInBits = sizeBits};

                            deviceCommand.AddParameter(parameter);
                        }
                    }
                }

                result = deviceCommand;
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecCommandStorageItem - GetDeviceCommandParameters", e);
            }

            return result;
        }

        #endregion

        #endregion
    }
}

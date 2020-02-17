using System;
using EltraCommon.Logger;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;

namespace EltraCloudStorage.Items
{
    class DataTypeStorageItem : StorageItem
    {
        public bool SetGetDataTypeId(DataType dataType, out int dataTypeId)
        {
            bool result = true;

            dataTypeId = -1;

            if (dataType!=null)
            { 
                if (!GetDataTypeId(dataType, out dataTypeId))
                {
                    if (GetDataTypeIdByTypeOnly(dataType, out dataTypeId))
                    {
                        result = UpdateDataType(dataTypeId, dataType);
                    }
                    else
                    {
                        if (AddDataType(dataType))
                        {
                            result = GetDataTypeId(dataType, out dataTypeId);
                        }
                    }
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - SetGetDataTypeId", "Data type cannot be null!");
                result = false;
            }

            return result;
        }

        private bool UpdateDataType(int dataTypeId, DataType dataType)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateDataType));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@data_type_id", dataTypeId));
                    command.Parameters.Add(new DbParameterWrapper("@size_bits", dataType.SizeInBits));
                    command.Parameters.Add(new DbParameterWrapper("@size_bytes", dataType.SizeInBytes));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UpdateDataType", e);

                result = false;
            }

            return result;
        }

        private bool AddDataType(DataType dataType)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertDataType));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@type", (int)dataType.Type));
                    command.Parameters.Add(new DbParameterWrapper("@size_bits", dataType.SizeInBits));
                    command.Parameters.Add(new DbParameterWrapper("@size_bytes", dataType.SizeInBytes));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddDataType", e);

                result = false;
            }

            return result;
        }

        private bool GetDataTypeId(DataType dataType, out int dataTypeId)
        {
            bool result = false;

            dataTypeId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDataType));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@type", (int)dataType.Type));
                    command.Parameters.Add(new DbParameterWrapper("@size_bits", dataType.SizeInBits));
                    command.Parameters.Add(new DbParameterWrapper("@size_bytes", dataType.SizeInBytes));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult!=null && !(queryResult is DBNull))
                    {
                        dataTypeId = Convert.ToInt32(queryResult);
                        result = dataTypeId > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetDataTypeId", e);
            }

            return result;
        }

        private bool GetDataTypeIdByTypeOnly(DataType dataType, out int dataTypeId)
        {
            bool result = false;

            dataTypeId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDataTypeByTypeOnly));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@type", (int)dataType.Type));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        dataTypeId = Convert.ToInt32(queryResult);
                        result = dataTypeId > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetDataTypeIdByTypeOnly", e);
            }

            return result;
        }
    }
}

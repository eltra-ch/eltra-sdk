using System;
using EltraCommon.Logger;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using System.Text;
using System.Collections.Generic;

namespace EltraCloudStorage.Items
{
    class ParameterUpdateStorageItem : StorageItem
    {
        private readonly DataTypeStorageItem _dataTypeStorageItem;
        private readonly DeviceStorageItem _deviceStorageItem;
        private readonly DeviceDescriptionStorageItem _deviceDescriptionStorage;

        public ParameterUpdateStorageItem(DeviceStorageItem deviceStorageItem)
        {
            _deviceStorageItem = deviceStorageItem;
            
            _deviceDescriptionStorage = new DeviceDescriptionStorageItem { Engine = Engine };
            _dataTypeStorageItem = new DataTypeStorageItem { Engine = Engine };

            AddChild(_dataTypeStorageItem);
            AddChild(_deviceDescriptionStorage);
        }

        private bool AddParameter(int deviceId, ParameterUpdate parameterUpdate)
        {
            bool result = false;
            var parameter = parameterUpdate?.Parameter;
            var dataType = parameter?.DataType;

            if (_dataTypeStorageItem.SetGetDataTypeId(dataType, out int dataTypeId))
            {
                if (_deviceDescriptionStorage.GetDeviceDescriptionIdFromDeviceId(deviceId, out int deviceDescriptionId))
                {
                    try
                    {
                        string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertParameter));

                        using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                        {
                            command.Parameters.Add(new DbParameterWrapper("@device_description_idref", deviceDescriptionId));
                            command.Parameters.Add(new DbParameterWrapper("@index", parameter.Index));
                            command.Parameters.Add(new DbParameterWrapper("@sub_index", parameter.SubIndex));
                            command.Parameters.Add(new DbParameterWrapper("@unique_id", parameter.UniqueId));
                            command.Parameters.Add(new DbParameterWrapper("@data_type_id", dataTypeId));

                            if (command.ExecuteNonQuery() > 0)
                            {
                                result = true;
                            }
                            else
                            {
                                MsgLogger.WriteError("ParameterUpdateStorageItem - AddParameter", $"data type {dataType.Type} coudn't be created!");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MsgLogger.Exception("ParameterUpdateStorageItem - AddParameter", e);
                    }
                }
                else
                {
                    MsgLogger.WriteError("ParameterUpdateStorageItem - AddParameter", $"cannot find device description for device_id={deviceId}");
                }
            }
            else
            {
                MsgLogger.WriteError("ParameterUpdateStorageItem - AddParameter", $"data type {dataType.Type} coudn't be created!");
            }

            return result;
        }

        private bool AddParameterValue(int deviceId, int parameterId, ParameterUpdate parameterUpdate)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertParameterValue));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@parameter_id", parameterId));
                    command.Parameters.Add(new DbParameterWrapper("@device_id", deviceId));
                    command.Parameters.Add(new DbParameterWrapper("@actual_value", parameterUpdate.Parameter.ActualValue.Value));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ParameterUpdateStorageItem - AddParameterValue", e);
            }

            return result;
        }

        private bool GetParameterId(int deviceId, ushort index, byte subIndex, out int id)
        {
            bool result = false;

            id = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectParameter));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@device_id", deviceId));
                    command.Parameters.Add(new DbParameterWrapper("@index", index));
                    command.Parameters.Add(new DbParameterWrapper("@sub_index", subIndex));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        id = Convert.ToInt32(queryResult);

                        result = id > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ParameterUpdateStorageItem - GetParameterId", e);
            }

            return result;
        }
        
        public bool UpdateParameter(ParameterUpdate parameterUpdate)
        {
            bool result = false;

            var transaction = Connection?.BeginTransaction();

            if (transaction != null)
            {
                if (_deviceStorageItem.GetDeviceIdBySerialNumber(parameterUpdate.SerialNumber, out var deviceId) && deviceId > 0)
                {
                    var index = parameterUpdate.Parameter.Index;
                    var subIndex = parameterUpdate.Parameter.SubIndex;

                    if (GetParameterId(deviceId, index, subIndex, out var parameterId) && parameterId > 0)
                    {
                        result = AddParameterValue(deviceId, parameterId, parameterUpdate);
                    }
                    else if (AddParameter(deviceId, parameterUpdate))
                    {
                        if (GetParameterId(deviceId, index, subIndex, out parameterId) && parameterId > 0)
                        {
                            result = AddParameterValue(deviceId, parameterId, parameterUpdate);
                        }
                    }
                }

                if (result)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }

            return result;
        }

        public ParameterValue GetParameterValue(ulong serialNumber, ushort index, byte subIndex)
        {
            ParameterValue result = null;
            int parameterId = 0;
            var startLog = MsgLogger.BeginTimeMeasure();
            
            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetParameterValueId));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@index", index));
                    command.Parameters.Add(new DbParameterWrapper("@subindex", subIndex));

                    var queryResult = command.ExecuteScalar();

                    if(queryResult!=null && !(queryResult is DBNull))
                    {
                        parameterId = Convert.ToInt32(queryResult);
                    }                                                 
                }

                if (parameterId > 0)
                {
                    commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetParameterValue));

                    using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                    {
                        command.Parameters.Add(new DbParameterWrapper("@parameterId", parameterId));

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                byte[] actualValueEncoded = (byte[])reader.GetValue(0);
                                var actualValueText = Encoding.UTF8.GetString(actualValueEncoded);
                                var actualValue = Convert.FromBase64String(actualValueText);

                                result = new ParameterValue(actualValue) { Modified = reader.GetDateTime(1) };
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetParameterValue", e);
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - GetParameterValue", startLog, $"Serial number = {serialNumber}, index={index}, subindex={subIndex}");
            
            return result;
        }

        public List<ParameterValue> GetParameterHistory(int deviceId, string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectParameterHistory));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uniqueId", uniqueId));
                    command.Parameters.Add(new DbParameterWrapper("@deviceId", deviceId));
                    command.Parameters.Add(new DbParameterWrapper("@from", from));
                    command.Parameters.Add(new DbParameterWrapper("@to", to));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] actualValueEncoded = (byte[])reader.GetValue(0);
                            var actualValueText = Encoding.UTF8.GetString(actualValueEncoded);
                            var actualValue = Convert.FromBase64String(actualValueText);

                            var parameterValue = new ParameterValue(actualValue) { Modified = reader.GetDateTime(1) };

                            result.Add(parameterValue);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ParameterUpdateStorageItem - GetParameterHistory", e);
            }

            return result;
        }

        public List<ParameterUniqueIdValuePair> GetParameterPairHistory(int deviceId, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            var result = new List<ParameterUniqueIdValuePair>();

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectParameterPairHistory));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uniqueId1", uniqueId1));
                    command.Parameters.Add(new DbParameterWrapper("@uniqueId2", uniqueId2));
                    command.Parameters.Add(new DbParameterWrapper("@deviceId", deviceId));
                    command.Parameters.Add(new DbParameterWrapper("@from", from));
                    command.Parameters.Add(new DbParameterWrapper("@to", to));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string uniqueId = reader.GetString(0);
                            byte[] actualValueEncoded = (byte[])reader.GetValue(1);
                            var actualValueText = Encoding.UTF8.GetString(actualValueEncoded);
                            var actualValue = Convert.FromBase64String(actualValueText);

                            var ipv = new ParameterUniqueIdValuePair
                            {
                                UniqueId = uniqueId,

                                Value = new ParameterValue(actualValue) 
                                { 
                                    Modified = reader.GetDateTime(2) 
                                }
                            };

                            result.Add(ipv);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ParameterUpdateStorageItem - GetParameterPairHistory", e);
            }

            return result;
        }
    }
}

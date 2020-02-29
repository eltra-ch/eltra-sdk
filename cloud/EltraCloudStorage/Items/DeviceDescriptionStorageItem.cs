using System;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;
using EltraCommon.Logger;

namespace EltraCloudStorage.Items
{
    class DeviceDescriptionStorageItem : StorageItem
    {
        private readonly DeviceVersionStorageItem _deviceVersionStorage;

        public DeviceDescriptionStorageItem()
        {
            _deviceVersionStorage = new DeviceVersionStorageItem() { Engine = Engine };

            AddChild(_deviceVersionStorage);
        }
        
        public bool Exists(ulong serialNumber, string hashCode)
        {
            bool result = false;

            if (GetDeviceDescriptionId(hashCode, out var deviceDescriptionId))
            {
                result = UpdateDeviceDescriptionReference(serialNumber, deviceDescriptionId);
            }

            return result;
        }

        private bool GetDeviceDescriptionId(string hashCode, out int deviceDescriptionId)
        {
            bool result = false;

            deviceDescriptionId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.DeviceDescriptionExist));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@hash_code", hashCode));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        deviceDescriptionId = Convert.ToInt32(queryResult);

                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Exists", e);
            }

            return result;
        }

        public bool Changed(DeviceDescriptionPayload deviceDescription, out int deviceDescriptionId)
        {
            bool result = false;

            deviceDescriptionId = 0;

            try
            {
                if (_deviceVersionStorage.GetDeviceVersionId(deviceDescription.Version, out var versionId))
                {
                    string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.DeviceDescriptionChanged));

                    using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                    {
                        command.Parameters.Add(new DbParameterWrapper("@versionId", versionId));
                        command.Parameters.Add(new DbParameterWrapper("@hash_code", deviceDescription.HashCode));

                        var queryResult = command.ExecuteScalar();

                        if (queryResult != null && !(queryResult is DBNull))
                        {
                            deviceDescriptionId = Convert.ToInt32(queryResult);

                            result = deviceDescriptionId > 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Changed", e);
            }
            
            return result;
        }

        private bool UpdateDeviceDescription(int deviceDescriptionId, DeviceDescriptionPayload deviceDescription)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateDeviceDescription));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@device_description_id", deviceDescriptionId));
                    command.Parameters.Add(new DbParameterWrapper("@content", deviceDescription.Content));
                    command.Parameters.Add(new DbParameterWrapper("@encoding", deviceDescription.Encoding));
                    command.Parameters.Add(new DbParameterWrapper("@hash_code", deviceDescription.HashCode));

                    var queryResult = command.ExecuteNonQuery();

                    if (queryResult > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UpdateDeviceDescription", e);
            }

            return result;
        }

        public bool Upload(DeviceDescriptionPayload deviceDescription)
        {
            bool result = false;
            
            if(Changed(deviceDescription, out var deviceDescriptionId))
            {
                result = UpdateDeviceDescription(deviceDescriptionId, deviceDescription);
            }
            else
            {
                if(!_deviceVersionStorage.DeviceVersionExists(deviceDescription.Version))
                {
                    _deviceVersionStorage.AddDeviceVersion(deviceDescription.Version);
                }
                
                if (_deviceVersionStorage.GetDeviceVersionId(deviceDescription.Version, out var deviceVersionId))
                { 
                    result = AddDeviceDescription(deviceDescription, deviceVersionId);
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Upload", $"cannot retrieve device version id ({deviceDescription.Version})");
                }
            }

            if(GetDeviceDescriptionId(deviceDescription.HashCode, out deviceDescriptionId))
            {
                result = UpdateDeviceDescriptionReference(deviceDescription.SerialNumber, deviceDescriptionId);
            }

            return result;
        }

        private bool UpdateDeviceDescriptionReference(ulong serialNumber, int deviceDescriptionId)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateDeviceDescriptionReference));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@device_description_id", deviceDescriptionId));

                    var queryResult = command.ExecuteNonQuery();

                    if (queryResult > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddDeviceDescription", e);
            }

            return result;
        }

        private bool AddDeviceDescription(DeviceDescriptionPayload deviceDescription, int deviceVersionId)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.AddDeviceDescription));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@device_version_id", deviceVersionId));
                    command.Parameters.Add(new DbParameterWrapper("@content", deviceDescription.Content));
                    command.Parameters.Add(new DbParameterWrapper("@encoding", deviceDescription.Encoding));
                    command.Parameters.Add(new DbParameterWrapper("@hash_code", deviceDescription.HashCode));

                    var queryResult = command.ExecuteNonQuery();

                    if (queryResult > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddDeviceDescription", e);
            }

            return result;
        }

        public bool GetDeviceDescriptionIdByVersion(int deviceVersionId, out int id)
        {
            bool result = false;
            string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetDeviceDescriptionIdByVersionId));

            id = 0;

            using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
            {
                command.Parameters.Add(new DbParameterWrapper("@versionId", deviceVersionId));

                var queryResult = command.ExecuteScalar();

                if (queryResult != null && !(queryResult is DBNull))
                {
                    id = Convert.ToInt32(queryResult);

                    result = id > 0;
                }
            }

            return result;
        }

        public bool GetDeviceDescriptionIdFromDeviceId(int deviceId, out int id)
        {
            bool result = false;
            string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetDeviceDescriptionIdByDeviceId));

            id = 0;

            using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
            {
                command.Parameters.Add(new DbParameterWrapper("@device_id", deviceId));

                var queryResult = command.ExecuteScalar();

                if (queryResult != null && !(queryResult is DBNull))
                {
                    id = Convert.ToInt32(queryResult);

                    result = id > 0;
                }
            }

            return result;
        }

        private DeviceDescriptionPayload GetDeviceDescription(int deviceVersionId)
        {
            DeviceDescriptionPayload result = null;
            string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetDeviceDescription));

            using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
            {
                command.Parameters.Add(new DbParameterWrapper("@versionId", deviceVersionId));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new DeviceDescriptionPayload
                        {
                            Content = reader.GetString(1),
                            Encoding = reader.GetString(2),
                            Modified = reader.GetDateTime(4)
                        };
                    }
                }
            }

            return result;
        }

        public DeviceDescriptionPayload Download(DeviceVersion deviceVersion)
        {
            DeviceDescriptionPayload result = null;

            try
            {
                if (_deviceVersionStorage.GetDeviceVersionId(deviceVersion, out var deviceVersionId))
                {
                    result = GetDeviceDescription(deviceVersionId);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Download", e);
            }

            return result;
        }
    }
}

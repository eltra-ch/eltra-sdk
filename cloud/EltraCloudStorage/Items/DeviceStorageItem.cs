using System;
using System.Collections.Generic;
using EltraCommon.Logger;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;

namespace EltraCloudStorage.Items
{
    class DeviceStorageItem : StorageItem
    {
        private readonly DeviceVersionStorageItem _deviceVersionStorage;
        private readonly DeviceDescriptionStorageItem _deviceDescriptionStorage;

        public DeviceStorageItem()
        {
            _deviceVersionStorage = new DeviceVersionStorageItem() { Engine = Engine };
            _deviceDescriptionStorage = new DeviceDescriptionStorageItem() { Engine = Engine };

            AddChild(_deviceVersionStorage);
            AddChild(_deviceDescriptionStorage);
        }

        public bool GetDeviceIdBySerialNumber(EltraDevice device, out int id)
        {
            bool result = false;

            id = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDeviceIdBySerialNumber));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", device.Identification.SerialNumber));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        id = Convert.ToInt32(queryResult);
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetDeviceIdBySerialNumber", e);
            }

            return result;
        }

        public bool GetDeviceIdBySerialNumber(ulong serialNumber, out int id)
        {
            bool result = false;

            id = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDeviceIdBySerialNumber));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", serialNumber));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        id = Convert.ToInt32(queryResult);
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetDeviceIdBySerialNumber", e);
            }

            return result;
        }

        private bool EnsureDeviceVersionExists(EltraDevice device, out int id)
        {
            bool result = true;
            var version = device.Version;

            id = 0;

            if (!_deviceVersionStorage.DeviceVersionExists(version))
            {
                result = _deviceVersionStorage.AddDeviceVersion(version);
            }

            if (result)
            {
                result = _deviceVersionStorage.GetDeviceVersionId(version, out id);
            }

            return result;
        }

        private bool GetDeviceDescriptionId(EltraDevice device, out int id)
        {
            bool result = false;
            var version = device?.Version;

            id = 0;

            if (_deviceVersionStorage.GetDeviceVersionId(version, out var deviceVersionId))
            {
                result = _deviceDescriptionStorage.GetDeviceDescriptionIdByVersion(deviceVersionId, out id);                
            }
            
            return result;
        }

        public bool AddDevice(EltraDevice device)
        {
            bool result = false;

            if (Connection.IsOpen)
            {
                if (EnsureDeviceVersionExists(device, out var deviceVersionIdref))
                {
                    if (GetDeviceDescriptionId(device, out var deviceDescriptionIdRef))
                    {
                        result = AddDevice(device, deviceVersionIdref, deviceDescriptionIdRef);
                    }
                }
            }

            return result;
        }

        public bool DeviceExists(EltraDevice device)
        {
            bool result = false;

            if (Connection.IsOpen)
            {
                result = GetDeviceIdBySerialNumber(device, out var id) && id > 0;
            }

            return result;
        }

        internal bool DeviceExists(ulong serialNumber)
        {
            bool result = false;

            if (Connection.IsOpen)
            {
                result = GetDeviceIdBySerialNumber(serialNumber, out var id) && id > 0;
            }

            return result;
        }

        private bool AddDevice(EltraDevice device, int deviceVersionIdref, int deviceDescriptionId)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertDevice));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", device.Identification.SerialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@device_name", device.Name));
                    command.Parameters.Add(new DbParameterWrapper("@device_description_idref", deviceDescriptionId));

                    string productName = string.Empty;

                    if(!string.IsNullOrEmpty(device.ProductName))
                    {
                        productName = device.ProductName;
                    }
                    
                    command.Parameters.Add(new DbParameterWrapper("@product_name", productName));

                    command.Parameters.Add(new DbParameterWrapper("@device_version_idref", deviceVersionIdref));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)device.Status));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddDevice", e);
            }

            return result;
        }

        public bool UpdateDeviceStatus(EltraDevice device, DeviceStatus status)
        {
            bool result = false;

            if (Connection.IsOpen)
            {
                if (GetDeviceIdBySerialNumber(device, out var id) && id > 0)
                {
                    result = ChangeDeviceStatus(id, status);
                }
            }

            return result;
        }

        public bool UpdateDeviceStatus(ulong serialNumber, DeviceStatus status)
        {
            bool result = false;

            if (Connection.IsOpen)
            {
                if (GetDeviceIdBySerialNumber(serialNumber, out var id) && id > 0)
                {
                    result = ChangeDeviceStatus(id, status);
                }
            }

            return result;
        }

        private bool ChangeDeviceStatus(int id, DeviceStatus status)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateDeviceStatus));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@device_id", id));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ChangeDeviceStatus", e);
            }

            return result;
        }
    }
}

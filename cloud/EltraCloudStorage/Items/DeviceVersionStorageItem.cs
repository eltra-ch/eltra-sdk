using EltraCloudContracts.Contracts.Devices;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;
using EltraCommon.Logger;
using System;

namespace EltraCloudStorage.Items
{
    class DeviceVersionStorageItem : StorageItem
    {
        public bool DeviceVersionExists(DeviceVersion deviceVersion)
        {
            return GetDeviceVersionId(deviceVersion, out int id) && id > 0;
        }

        public bool GetDeviceVersionId(DeviceVersion deviceVersion, out int id)
        {
            bool result = false;

            id = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDeviceVersionId));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@hardware_version", deviceVersion.HardwareVersion));
                    command.Parameters.Add(new DbParameterWrapper("@software_version", deviceVersion.SoftwareVersion));
                    command.Parameters.Add(new DbParameterWrapper("@application_number", deviceVersion.ApplicationNumber));
                    command.Parameters.Add(new DbParameterWrapper("@application_version", deviceVersion.ApplicationVersion));

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
                MsgLogger.Exception("DeviceVersionStorageItem - GetDeviceVersionId", e);
            }

            return result;
        }

        public bool AddDeviceVersion(DeviceVersion deviceVersion)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertDeviceVersion));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@hardware_version", deviceVersion.HardwareVersion));
                    command.Parameters.Add(new DbParameterWrapper("@software_version", deviceVersion.SoftwareVersion));
                    command.Parameters.Add(new DbParameterWrapper("@application_number", deviceVersion.ApplicationNumber));
                    command.Parameters.Add(new DbParameterWrapper("@application_version", deviceVersion.ApplicationVersion));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("DeviceVersionStorageItem - AddDeviceVersion", e);
            }

            return result;
        }
    }
}

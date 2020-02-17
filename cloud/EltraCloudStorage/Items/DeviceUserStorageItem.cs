using System;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Users;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;

namespace EltraCloudStorage.Items
{
    class DeviceUserStorageItem : StorageItem
    {
        public bool AddDeviceUser(User deviceUser)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertDeviceUser));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@login_name", deviceUser.AuthData.Login));
                    command.Parameters.Add(new DbParameterWrapper("@user_name", deviceUser.AuthData.Name));
                    command.Parameters.Add(new DbParameterWrapper("@password", deviceUser.AuthData.Password));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)deviceUser.Status));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddDeviceUser", e);
            }

            return result;
        }

        public bool GetDeviceUserIdByLoginName(User deviceUser, out int deviceUserId)
        {
            bool result = false;

            deviceUserId = 0;

            if (deviceUser != null)
            {
                result = GetDeviceUserIdByLoginName(deviceUser.AuthData.Login, out deviceUserId);
            }

            return result;
        }

        public bool GetDeviceUserIdByLoginName(string loginName, out int deviceUserId)
        {
            bool result = false;

            deviceUserId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDeviceUserIdByLoginName));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@login_name", loginName));

                    var queryResult = command.ExecuteScalar();
                    
                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        deviceUserId = Convert.ToInt32(queryResult);

                        result = deviceUserId > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetDeviceUserIdByLoginName", e);
            }

            return result;
        }

        public bool DeviceUserExists(User deviceUser)
        {
            bool result = GetDeviceUserIdByLoginName(deviceUser, out var deviceUserId) && deviceUserId > 0;

            return result;
        }

        public bool UpdateDeviceUserStatus(User deviceUser)
        {
            bool result = false;
            
            if (GetDeviceUserIdByLoginName(deviceUser, out var deviceUserId) && deviceUserId > 0)
            {
                try
                {
                    string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateDeviceUserStatus));

                    using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                    {
                        command.Parameters.Add(new DbParameterWrapper("@device_user_id", deviceUserId));
                        command.Parameters.Add(new DbParameterWrapper("@status", (int)deviceUser.Status));

                        var queryResult = command.ExecuteNonQuery();

                        if (queryResult > 0)
                        {
                            result = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - UpdateDeviceUserStatus", e);
                }
            }

            return result;
        }
    }
}

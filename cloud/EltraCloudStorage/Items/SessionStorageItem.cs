using System;
using System.Collections.Generic;
using System.Data;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.Contracts.Users;
using EltraCloudStorage.DataSource.Connection;
using EltraCloudStorage.DataSource.Connection.Factory;
using EltraCloudStorage.DataSource.Reader;
using EltraCloudContracts.Contracts.ToolSet;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudStorage.Items
{
    class SessionStorageItem : StorageItem, IDisposable
    {
        #region Private fields

        private string _connectionString;
        private readonly DeviceStorageItem _deviceStorage;
        private readonly DeviceUserStorageItem _userStorage;
        private readonly ParameterUpdateStorageItem _parameterUpdateStorage;
        private readonly DeviceCommandSetItem _deviceCommandSetStorage;
        private readonly DeviceToolSetItem _deviceToolSetStorage;
        private readonly ExecCommandStorageItem _execCommandStorage;
        private readonly LocationStorageItem _locationStorage;

        private readonly DeviceDescriptionStorageItem _deviceDescriptionStorage;
        #endregion

        #region Constructors

        public SessionStorageItem()
        {
            _deviceStorage = new DeviceStorageItem { Engine = Engine };
            _userStorage = new DeviceUserStorageItem { Engine = Engine };
            _parameterUpdateStorage = new ParameterUpdateStorageItem(_deviceStorage) { Engine = Engine };
            _deviceCommandSetStorage = new DeviceCommandSetItem { Engine = Engine };
            _deviceToolSetStorage = new DeviceToolSetItem { Engine = Engine };
            _execCommandStorage = new ExecCommandStorageItem { Engine = Engine, DeviceCommandSet = _deviceCommandSetStorage };
            _locationStorage = new LocationStorageItem { Engine = Engine };
            _deviceDescriptionStorage = new DeviceDescriptionStorageItem { Engine = Engine };

            AddChild(_deviceStorage);
            AddChild(_userStorage);
            AddChild(_parameterUpdateStorage);
            AddChild(_deviceCommandSetStorage);
            AddChild(_deviceToolSetStorage);
            AddChild(_execCommandStorage);
            AddChild(_locationStorage);
            AddChild(_deviceDescriptionStorage);
        }

        #endregion

        #region Properties

        public string ConnectionString
        {
            get => _connectionString;
            set { _connectionString = value; OnConnectionStringChanged();}
        }

        #endregion

        #region Methods

        #region Session

        private List<DeviceTool> GetTools(int deviceId)
        {
            var result = new List<DeviceTool>();

            try
            {
                //TODO - change sql_mode if necessary
                //SET sql_mode=(SELECT REPLACE(@@sql_mode,'ONLY_FULL_GROUP_BY',''));
                //SELECT @@sql_mode;

                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetDeviceTools));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@device_id", deviceId));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)DeviceToolStatus.Enabled));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader != null && reader.Read())
                        {
                            var tool = new DeviceTool
                            {
                                Name = reader.GetString(0),
                                Uuid = reader.GetString(1),
                                Status = DeviceToolStatus.Enabled
                            };

                            result.Add(tool);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetTools", e); ;
            }

            return result;
        }

        private List<DeviceCommand> GetCommands(int deviceId)
        {
            var result = new List<DeviceCommand>();

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetDeviceCommands));

                var sqlDeviceCommands = new Dictionary<int, DeviceCommand>();
                
                using (var sqlCommand = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    sqlCommand.Parameters.Add(new DbParameterWrapper("@device_id", deviceId));

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader != null && reader.Read())
                        {
                            var commandId = reader.GetInt32(0);
                            
                            DeviceCommand deviceCommand = null; 
                            
                            if (!sqlDeviceCommands.ContainsKey(commandId))
                            {
                                deviceCommand = new DeviceCommand
                                {
                                    Name = reader.GetString(1)
                                };

                                sqlDeviceCommands.Add(commandId, deviceCommand);
                            }
                            else
                            {
                                deviceCommand = sqlDeviceCommands[commandId];
                            }

                            if (deviceCommand != null)
                            {
                                var parameter = new DeviceCommandParameter();

                                var parameterName = reader.GetString(2);
                                var direction = reader.GetInt32(3);
                                var dataType = reader.GetInt32(4);
                                var sizeInBytes = reader.GetUInt32(5);

                                parameter.DataType = new DataType(dataType, sizeInBytes);
                                parameter.Name = parameterName;
                                parameter.Type = (ParameterType)direction;

                                deviceCommand.AddParameter(parameter);
                            }
                        }
                    }
                }

                foreach(var sqlDeviceCommand in sqlDeviceCommands)
                {
                    result.Add(sqlDeviceCommand.Value);
                }

            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetTools", e); ;
            }

            return result;
        }

        public List<EltraDevice> GetSessionDevices(string uuid)
        {
            var result = new List<EltraDevice>();
            var sqlDevices = new List<ValueTuple<int, EltraDevice>>();

            try
            {
                //TODO - change sql_mode if necessary
                //SET sql_mode=(SELECT REPLACE(@@sql_mode,'ONLY_FULL_GROUP_BY',''));
                //SELECT @@sql_mode;

                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDevicesbySessionId));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)SessionStatus.Online));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader != null && reader.Read())
                        {
                            var device = new EltraDevice();
                            var deviceId = reader.GetInt32(0);
                            var deviceName = reader.GetString(1);
                            var serialNumber = reader.GetUInt32(2);
                            var deviceStatus = (DeviceStatus)reader.GetInt32(3);
                            var modified = reader.GetDateTime(4);
                            var created = reader.GetDateTime(5);

                            device.Name = deviceName;
                            device.SessionUuid = uuid;

                            device.Identification.SerialNumber = serialNumber;

                            device.Status = deviceStatus;
                            device.Modified = modified;
                            device.Created = created;

                            var version = new DeviceVersion()
                            {
                                HardwareVersion = (ushort)reader.GetInt32(6),
                                SoftwareVersion = (ushort)reader.GetInt32(7),
                                ApplicationNumber = (ushort)reader.GetInt32(8),
                                ApplicationVersion = (ushort)reader.GetInt32(9)
                            };

                            device.Version = version;

                            sqlDevices.Add(new ValueTuple<int, EltraDevice>(deviceId, device));
                        }
                    }

                    foreach (var sqlDevice in sqlDevices)
                    {
                        UpdateToolSet(sqlDevice);

                        UpdateCommandSet(sqlDevice);

                        result.Add(sqlDevice.Item2);
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetSessionDevices", e);;
            }

            return result;
        }

        private bool SessionLinkExists(int master_session_id, int slave_session_id, out int sessionLinkId)
        {
            bool result = false;

            sessionLinkId = -1;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetSessionLink));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@master_session_id", master_session_id));
                    command.Parameters.Add(new DbParameterWrapper("@slave_session_id", slave_session_id));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult!=null && !(queryResult is DBNull))
                    {
                        sessionLinkId = Convert.ToInt32(queryResult);
                        
                        if(sessionLinkId>0)
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SessionLinkExists", e);
            }

            return result;
        }

        public List<string> GetLinkedSessionUuids(string uuid, bool isMaster)
        {
            var result = new List<string>();

            try
            {
                string commandText = string.Empty;

                if (isMaster)
                {
                    commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetLinkedMasterSessionUuids));
                }
                else
                {
                    commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetLinkedSlaveSessionUuids));
                }
                
                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));
                    command.Parameters.Add(new DbParameterWrapper("@status", SessionStatus.Online));

                    using (var reader = command.ExecuteReader())
                    {
                        while(reader!=null && reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }   
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetLinkedMasterSessionUuids", e);
            }

            return result;
        }

        public bool CreateSessionLink(string uuid, List<Session> sessions)
        {
            bool result = false;

            try
            {
                var transaction = Connection.BeginTransaction();

                if (GetSessionIdByUuid(uuid, out var slave_session_id))
                {
                    foreach(var session in sessions)
                    {
                        if (GetSessionIdByUuid(session.Uuid, out var master_session_id))
                        {
                            if (!SessionLinkExists(master_session_id, slave_session_id, out var sessionLinkId))
                            {
                                result = AddSessionLink(slave_session_id, master_session_id);
                            }
                            else
                            {
                                result = UpdateSessionLinkStatus(sessionLinkId, SessionStatus.Online);

                                if(result)
                                {
                                    MsgLogger.WriteDebug($"{GetType().Name} - CreateSessionLink", $"link between session {uuid} and {session.Uuid} exists");
                                }
                                else
                                {
                                    MsgLogger.WriteError($"{GetType().Name} - CreateSessionLink", $"link update between session {uuid} and {session.Uuid} failed");
                                }
                            }
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - CreateSessionLink", $"cannot retrieve session {uuid} db key, skip processing");
                            result = false;
                            break;
                        }
                    }
                }
                else
                {
                    MsgLogger.WriteWarning($"{GetType().Name} - CreateSessionLink", $"slave session '{uuid}' id, cannot be found, not registered yet?");
                    result = false;
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CreateSessionLink", e);
            }

            return result;
        }

        private bool UpdateSessionLinkStatus(int sessionLinkId, SessionStatus status)
        {
            bool result = false;
            string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.SetSessionLinkStatus));

            using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
            {
                command.Parameters.Add(new DbParameterWrapper("@session_link_id", sessionLinkId));
                command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                if (command.ExecuteNonQuery() > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public bool UpdateSessionLinksStatus(string uuid, SessionStatus status)
        {
            bool result = false;
            string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.SetSessionLinkStatusBySessionId));

            try
            {
                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                    command.ExecuteNonQuery();

                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UpdateSessionLinkStatusBySessionId", e);
            }

            return result;
        }

        private bool AddSessionLink(int slave_session_id, int master_session_id)
        {
            bool result = false;
            string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertSessionLink));
            
            using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
            {
                command.Parameters.Add(new DbParameterWrapper("@master_session_id", master_session_id));
                command.Parameters.Add(new DbParameterWrapper("@slave_session_id", slave_session_id));
                command.Parameters.Add(new DbParameterWrapper("@status", (int)SessionStatus.Online));

                if (command.ExecuteNonQuery() > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        private void UpdateToolSet(ValueTuple<int, EltraDevice> sqlDevices)
        {
            var tools = GetTools(sqlDevices.Item1);

            foreach (var tool in tools)
            {
                var device = sqlDevices.Item2;

                device.ToolSet.AddTool(tool);
            }
        }

        private void UpdateCommandSet(ValueTuple<int, EltraDevice> sqlDevices)
        {
            var commands = GetCommands(sqlDevices.Item1);

            foreach (var command in commands)
            {
                var device = sqlDevices.Item2;

                device.CommandSet.AddCommand(command);
            }
        }

        public bool GetSessionDevicesCount(string uuid, out int count)
        {
            bool result = false;

            count = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDeviceCountbySessionId));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)SessionStatus.Online));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        count = Convert.ToInt32(queryResult);    
                    }

                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public Session GetDeviceSession(ulong serialNumber, SessionStatus status)
        {
            Session result = null;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSessionBySerialNumber));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serialNumber", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader != null && reader.Read())
                        {
                            var sessionUuid = reader.GetString(0);
                            var sessionStatus = (SessionStatus)reader.GetInt32(1);
                            var sessionModified = reader.GetDateTime(2);
                            var sessionCreated = reader.GetDateTime(3);
                            var userLogin = reader.GetString(4);
                            string userName = string.Empty;

                            if (!reader.IsDBNull(5))
                            {
                                userName = reader.GetString(5);
                            }

                            var userPassword = reader.GetString(6);
                            var userStatus = (UserStatus)reader.GetInt32(7);
                            var userModified = reader.GetDateTime(8);
                            var userCreated = reader.GetDateTime(9);

                            var ip = reader.GetString(10);
                            string countryCode = string.Empty;
                            string country = string.Empty;
                            string region = string.Empty;
                            string city = string.Empty;
                            double latitude = 0;
                            double longitude = 0;

                            if (!reader.IsDBNull(11))
                            {
                                countryCode = reader.GetString(11);
                            }

                            if (!reader.IsDBNull(12))
                            {
                                country = reader.GetString(12);
                            }

                            if (!reader.IsDBNull(13))
                            {
                                region = reader.GetString(13);
                            }

                            if (!reader.IsDBNull(14))
                            {
                                city = reader.GetString(14);
                            }

                            if (!reader.IsDBNull(15))
                            {
                                latitude = reader.GetDouble(15);
                            }

                            if (!reader.IsDBNull(16))
                            {
                                longitude = reader.GetDouble(16);
                            }

                            var session = new Session
                            {
                                Uuid = sessionUuid,
                                Modified = sessionModified,
                                Created = sessionCreated,
                                Status = sessionStatus,
                                User =
                                {
                                    AuthData = new UserAuthData {Login = userLogin, Name = userName, Password = userPassword},
                                    Status = userStatus,
                                    Modified = userModified,
                                    Created = userCreated
                                },
                                IpLocation =
                                {
                                    Ip = ip,
                                    CountryCode = countryCode,
                                    Country = country,
                                    Region = region,
                                    City = city,
                                    Latitude = latitude,
                                    Longitude = longitude
                                }
                            };

                            result = session;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public SessionStatus GetSessionStatus(string uuid)
        {
            SessionStatus result = SessionStatus.Undefined;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.GetSessionStatus));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader != null && reader.Read())
                        {
                            result = (SessionStatus)reader.GetInt32(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetSessionStatus", e); ;
            }

            return result;
        }

        public Session GetSession(string uuid)
        {
            Session result = null;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSessionByUuid));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader != null && reader.Read())
                        {
                            var sessionUuid = reader.GetString(0);
                            var sessionStatus = (SessionStatus)reader.GetInt32(1);
                            var sessionModified = reader.GetDateTime(2);
                            var sessionCreated = reader.GetDateTime(3);
                            var userLogin = reader.GetString(4);
                            string userName = string.Empty;

                            if(!reader.IsDBNull(5))
                            {
                                userName = reader.GetString(5);
                            }

                            var userPassword = reader.GetString(6);
                            var userStatus = (UserStatus)reader.GetInt32(7);
                            var userModified = reader.GetDateTime(8);
                            var userCreated = reader.GetDateTime(9);

                            var ip = reader.GetString(10);
                            string countryCode = string.Empty;
                            string country = string.Empty;
                            string region = string.Empty;
                            string city = string.Empty;
                            double latitude = 0;
                            double longitude = 0;

                            if (!reader.IsDBNull(11))
                            {
                                countryCode = reader.GetString(11);
                            }

                            if (!reader.IsDBNull(12))
                            {
                                country = reader.GetString(12);
                            }

                            if (!reader.IsDBNull(13))
                            {
                                region = reader.GetString(13);
                            }

                            if (!reader.IsDBNull(14))
                            {
                                city = reader.GetString(14);
                            }

                            if (!reader.IsDBNull(15))
                            {
                                latitude = reader.GetDouble(15);
                            }

                            if (!reader.IsDBNull(16))
                            {
                                longitude = reader.GetDouble(16);
                            }

                            var session = new Session
                            {
                                Uuid = sessionUuid,
                                Modified = sessionModified,
                                Created = sessionCreated,
                                Status = sessionStatus,
                                User =
                                {
                                    AuthData = new UserAuthData {Login = userLogin, Name = userName, Password = userPassword},
                                    Status = userStatus,
                                    Modified = userModified,
                                    Created = userCreated
                                },
                                IpLocation =
                                {
                                    Ip = ip,
                                    CountryCode = countryCode,
                                    Country = country,
                                    Region = region,
                                    City = city,
                                    Latitude = latitude,
                                    Longitude = longitude
                                }
                            };

                            result = session;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        private IpLocation ReadIpLocation(DbReaderWrapper reader, int startIndex)
        {
            IpLocation result = new IpLocation();

            var ip = reader.GetString(startIndex);

            string countryCode = string.Empty;
            string country = string.Empty;
            string region = string.Empty;
            string city = string.Empty;
            double latitude = 0;
            double longitude = 0;

            int index = startIndex + 1;

            if (!reader.IsDBNull(index))
            {
                countryCode = reader.GetString(index);
            }

            if (!reader.IsDBNull(++index))
            {
                country = reader.GetString(index);
            }

            if (!reader.IsDBNull(++index))
            {
                region = reader.GetString(index);
            }

            if (!reader.IsDBNull(++index))
            {
                city = reader.GetString(index);
            }

            if (!reader.IsDBNull(++index))
            {
                latitude = reader.GetDouble(index);
            }

            if (!reader.IsDBNull(++index))
            {
                longitude = reader.GetDouble(index);
            }

            result.Ip = ip;
            result.CountryCode = countryCode;
            result.Country = country;
            result.Region = region;
            result.City = city;
            result.Latitude = latitude;
            result.Longitude = longitude;
            
            return result;
        }

        public IEnumerable<Session> GetSessions(SessionStatus status, bool deviceOnly)
        {
            List<Session> result = new List<Session>();

            try
            {
                string commandText;

                if (deviceOnly)
                {
                    commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDeviceSessionsByStatus));
                }
                else
                {
                    commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSessionsByStatus));
                }

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader!=null && reader.Read())
                        {
                            var sessionUuid = reader.GetString(0);
                            var sessionModified = reader.GetDateTime(1);
                            var sessionCreated = reader.GetDateTime(2);
                            var sessionTimeout = reader.GetUInt32(3);
                            var userLogin = reader.GetString(4);
                            var userName = reader.GetString(5);
                            var userPassword = reader.GetString(6);
                            var userStatus = (UserStatus) reader.GetInt32(7);
                            var userModified = reader.GetDateTime(8);
                            var userCreated = reader.GetDateTime(9);

                            var ipLocation = ReadIpLocation(reader, 10);

                            var session = new Session
                            {
                                Uuid = sessionUuid,
                                Modified = sessionModified,
                                Created = sessionCreated,
                                Status = status,
                                Timeout = sessionTimeout,
                                User =
                                {
                                    AuthData = new UserAuthData {Login = userLogin, Name = userName, Password = userPassword},
                                    Status = userStatus,
                                    Modified = userModified,
                                    Created = userCreated
                                },
                                IpLocation = ipLocation
                            };
                            
                            result.Add(session);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public IEnumerable<Session> GetSessions(string login, string password, SessionStatus status)
        {
            List<Session> result = new List<Session>();

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSessionsByUserPassword));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));
                    command.Parameters.Add(new DbParameterWrapper("@login", login));
                    command.Parameters.Add(new DbParameterWrapper("@password", password));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var sessionUuid = reader.GetString(0);
                            var sessionModified = reader.GetDateTime(1);
                            var sessionCreated = reader.GetDateTime(2);
                            
                            var userName = reader.GetString(3);
                            
                            var userStatus = (UserStatus)reader.GetInt32(4);
                            var userModified = reader.GetDateTime(5);
                            var userCreated = reader.GetDateTime(6);

                            var ipLocation = ReadIpLocation(reader, 7);

                            var session = new Session
                            {
                                Uuid = sessionUuid,
                                Modified = sessionModified,
                                Created = sessionCreated,
                                Status = status,
                                User =
                                {
                                    AuthData = new UserAuthData {Login = login, Name = userName, Password = password},
                                    Status = userStatus,
                                    Modified = userModified,
                                    Created = userCreated
                                },
                                IpLocation = ipLocation
                            };

                            result.Add(session);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool IsDeviceUsedByAgent(string agentSesionUuid, ulong serialNumber)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.IsDeviceUsedByAgent));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@session_id", agentSesionUuid));
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)SessionStatus.Online));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult!=null && !(queryResult is DBNull))
                    {                    
                        result = true;
                    }                    
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - IsDeviceUsedByAgent", e);;
            }

            return result;
        }

        public List<Session> GetSessionsOlderThan(SessionStatus status, int minutes)
        {
            List<Session> result = new List<Session>();

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSessionOlderThanMin));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@minutes", minutes));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader != null && reader.Read())
                        {
                            var session = new Session {Uuid = reader.GetString(0), Modified = reader.GetDateTime(1)};

                            result.Add(session);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool AddSessionDevice(SessionDevice sessionDevice)
        {
            bool result = false;
            var transaction = Connection.BeginTransaction();

            if (transaction != null && sessionDevice != null)
            {
                var device = sessionDevice.Device;

                result = GetSessionIdByUuid(sessionDevice.SessionUuid, out int sessionId);

                if (GetSetDeviceId(device, out var deviceId))
                {
                    if (!SessionDeviceExists(sessionId, deviceId))
                    {
                        result = AddSessionDevice(device, sessionId, deviceId);
                    }
                    else
                    {
                        result = UpdateSessionDevice(device, sessionId, deviceId);
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

        public bool UpdateSessionStatus(string uuid, SessionStatus status)
        {
            bool result = false;

            try
            {
                if (GetSessionIdByUuid(uuid, out var sessionId) && sessionId > 0)
                {
                    string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateSessionStatusById));

                    using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                    {
                        command.Parameters.Add(new DbParameterWrapper("@session_id", sessionId));
                        command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                        var queryResult = command.ExecuteNonQuery();

                        if (queryResult > 0)
                        {
                            result = true;
                        }                        
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UpdateSessionStatus", e);;
            }

            return result;
        }

        public bool UpdateSession(Session session)
        {
            bool result = false;

            if (session != null)
            {
                var transaction = Connection?.BeginTransaction();

                if (transaction!=null)
                { 
                    var deviceUser = session.User;
                    int deviceUserId = 0;

                    if (deviceUser != null)
                    {
                        result = GetSetDeviceUserId(deviceUser, out deviceUserId);
                    }

                    var ipLocation = session.IpLocation;
                    int locationId = 0;

                    if (ipLocation != null)
                    {
                        result = GetSetDeviceLocationId(ipLocation, out locationId);
                    }

                    if (result)
                    {
                        result = SessionExists(session, deviceUserId) ? UpdateSessionStatus(session, deviceUserId) : AddSession(session, deviceUserId, locationId);
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
            }
            
            return result;
        }

        public bool SessionExists(string sessionId)
        {
            var result = GetSessionIdByUuid(sessionId, out var id) && id > 0;

            return result;
        }

        public bool SessionExists(Session session)
        {
            bool result = false;

            if (session != null)
            {
                var deviceUser = session.User;
                int deviceUserId = 0;

                if (deviceUser != null)
                {
                    result = GetSetDeviceUserId(deviceUser, out deviceUserId);
                }

                if (result)
                {
                    result = SessionExists(session, deviceUserId);
                }
            }

            return result;
        }

        public bool UpdateSessionStatus(Session session)
        {
            bool result = false;

            if (session != null)
            {
                var transaction = Connection?.BeginTransaction();

                if (transaction != null)
                {
                    var deviceUser = session.User;
                    int deviceUserId = 0;

                    if (deviceUser != null)
                    {
                        result = GetSetDeviceUserId(deviceUser, out deviceUserId);
                    }

                    if (result)
                    {
                        result = UpdateSessionStatus(session, deviceUserId);
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
            }

            return result;
        }

        public bool UpdateSessionStatus(string loginName, string uuid, SessionStatus sessionStatus)
        {
            bool result = false;
            var transaction = Connection?.BeginTransaction();

            if (_userStorage != null && _userStorage.GetDeviceUserIdByLoginName(loginName, out var deviceUserId))
            {
                result = UpdateSessionStatus(uuid, deviceUserId, sessionStatus);
            }

            if (result)
            {
                transaction?.Commit();
            }
            else
            {
                transaction?.Rollback();
            }

            return result;
        }

        #endregion

        #region Device Description
        
        public bool DeviceDescriptionExists(string hashCode)
        {
            bool result = false;

            if (_deviceDescriptionStorage != null)
            {
                result = _deviceDescriptionStorage.Exists(hashCode);
            }

            return result;
        }

        public bool UploadDeviceDescription(DeviceDescription deviceDescription)
        {
            bool result = false;

            if (_deviceDescriptionStorage != null)
            {
                result = _deviceDescriptionStorage.Upload(deviceDescription);
            }

            return result;
        }

        public DeviceDescription DownloadDeviceDescription(DeviceVersion version)
        {
            DeviceDescription result = null;

            if (_deviceDescriptionStorage != null)
            {
                result = _deviceDescriptionStorage.Download(version);
            }

            return result;
        }

        #endregion

        #region Device

        public EltraDevice GetDevice(ulong deviceSerialNumber, SessionStatus status)
        {
            EltraDevice result = null;
            
            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDevicebySerialNumber));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)status));
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", deviceSerialNumber));

                    EltraDevice device = null;
                    int deviceId = 0;

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader != null && reader.Read())
                        {
                            device = new EltraDevice();
                            deviceId = reader.GetInt32(1);

                            var sessionId = reader.GetString(0);
                            var deviceName = reader.GetString(2);
                            var serialNumber = reader.GetUInt32(3);
                            var deviceStatus = (DeviceStatus)reader.GetInt32(4);
                            var modified = reader.GetDateTime(5);
                            var created = reader.GetDateTime(6);

                            device.Name = deviceName;

                            device.Identification.SerialNumber = serialNumber;

                            device.Status = deviceStatus;
                            device.Modified = modified;
                            device.Created = created;

                            var version = new DeviceVersion()
                            {
                                HardwareVersion = (ushort)reader.GetInt32(7),
                                SoftwareVersion = (ushort)reader.GetInt32(8),
                                ApplicationNumber = (ushort)reader.GetInt32(9),
                                ApplicationVersion = (ushort)reader.GetInt32(10)
                            };

                            device.Version = version;
                            device.SessionUuid = sessionId;
                        }
                    }

                    if (device != null)
                    {
                        UpdateToolSet(new ValueTuple<int, EltraDevice>(deviceId, device));

                        UpdateCommandSet(new ValueTuple<int, EltraDevice>(deviceId, device));
                    }

                    result = device;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }
        
        public bool UpdateDeviceStatus(string userLogin, string sessionUuid, ulong deviceSerialNumber, DeviceStatus deviceStatus)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateSessionDeviceStatus));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", sessionUuid));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)deviceStatus));
                    command.Parameters.Add(new DbParameterWrapper("@login", userLogin));
                    command.Parameters.Add(new DbParameterWrapper("@serialNumber", deviceSerialNumber));

                    int queryResult = command.ExecuteNonQuery();

                    if (queryResult > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool RemoveDevice(ulong serialNumber)
        {
            bool result = false;

            if (_deviceStorage.DeviceExists(serialNumber))
            {
                result = _deviceStorage.UpdateDeviceStatus(serialNumber, DeviceStatus.Unregistered);
            }

            return result;
        }

        #endregion

        #region Parameters

        public bool UpdateParameter(ParameterUpdate parameterUpdate)
        {
            return _parameterUpdateStorage.UpdateParameter(parameterUpdate);
        }

        public ParameterValue GetParameterValue(ulong serialNumber, ushort index, byte subIndex)
        {
            ParameterValue result = _parameterUpdateStorage.GetParameterValue(serialNumber, index, subIndex);

            return result;
        }

        public List<ParameterValue> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();

            if (_deviceStorage.GetDeviceIdBySerialNumber(serialNumber, out var deviceId))
            {
                result = _parameterUpdateStorage.GetParameterHistory(deviceId, uniqueId, from, to);
            }
            else
            {
                MsgLogger.WriteError("SessionStorageItem - GetParameterHistory", $"serial number = {serialNumber}, cannot be identified as device!");
            }

            return result;
        }

        public List<ParameterUniqueIdValuePair> GetParameterPairHistory(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            var result = new List<ParameterUniqueIdValuePair>();

            if (_deviceStorage.GetDeviceIdBySerialNumber(serialNumber, out var deviceId))
            {
                result = _parameterUpdateStorage.GetParameterPairHistory(deviceId, uniqueId1, uniqueId2, from, to);
            }
            else
            {
                MsgLogger.WriteError("SessionStorageItem - GetParameterPairHistory", $"serial number = {serialNumber}, cannot be identified as device!");
            }

            return result;
        }

        #endregion

        #region Commands

        public bool SetCommand(ExecuteCommand executeCommand)
        {
            bool result = false;

            if (executeCommand != null)
            {
                var command = executeCommand.Command;

                if (GetSessionCommandIds(executeCommand.SessionUuid, executeCommand.SerialNumber, command.Name, out var sessionId, out var commandId))
                {
                    result = _execCommandStorage.Update(executeCommand.CommandUuid, sessionId, commandId, command);
                }
            }

            return result;
        }

        private bool GetSessionCommandIds(string sessionUuid, ulong serialNumber, string commandName, out int sessionId, out int commandId)
        {
            bool result = false;

            sessionId = 0;
            commandId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSessionCommandIds));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@session_uuid", sessionUuid));
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@command_name", commandName));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader != null && reader.Read())
                        {
                            sessionId = reader.GetInt32(0);
                            commandId = reader.GetInt32(1);

                            result = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool SetCommandStatus(ExecuteCommandStatus executeCommandStatus)
        {
            bool result = false;

            if (GetSessionCommandIds(executeCommandStatus.SessionUuid, executeCommandStatus.SerialNumber, executeCommandStatus.CommandName, out var sessionId, out var commandId))
            {
                result = _execCommandStorage.UpdateStatus(executeCommandStatus.CommandUuid, sessionId, commandId, executeCommandStatus.Status);
            }

            return result;
        }

        public bool SetCommandCommStatus(ExecuteCommandStatus executeCommandStatus)
        {
            bool result = false;

            if (GetSessionCommandIds(executeCommandStatus.SessionUuid, executeCommandStatus.SerialNumber, executeCommandStatus.CommandName, out var sessionId, out var commandId))
            {
                result = _execCommandStorage.UpdateCommStatus(executeCommandStatus.CommandUuid, sessionId, commandId, executeCommandStatus.CommStatus);
            }

            return result;
        }

        public ExecuteCommand PopCommand(string uuid, ulong serialNumber, ExecCommandStatus status)
        {
            var result = _execCommandStorage.PopCommand(uuid, serialNumber, status);

            return result;
        }

        public List<ExecuteCommand> PopCommands(ulong serialNumber, ExecCommandStatus status)
        {
            var result = _execCommandStorage.PopCommands(serialNumber, status);

            return result;
        }

        public List<ExecuteCommand> GetExecCommands(string sessionUuid, ExecCommandStatus[] status)
        {
            var result = _execCommandStorage.GetExecCommands(sessionUuid, status);

            return result;
        }

        public List<ExecuteCommand> GetExecCommands(List<string> commandUuids)
        {
            var result = _execCommandStorage.GetExecCommands(commandUuids);

            return result;
        }

        public List<ExecuteCommand> PopCommands(ulong[] serialNumber, ExecCommandStatus[] status)
        {
            var result = _execCommandStorage.PopCommands(serialNumber, status);

            return result;
        }

        public ExecuteCommandStatus GetCommandStatus(string uuid, string sessionUuid, ulong serialNumber, string commandName)
        {
            return _deviceCommandSetStorage.GetCommandStatus(uuid, sessionUuid, serialNumber, commandName);
        }

        #endregion

        #region Private 

        private bool GetSetDeviceUserId(User deviceUser, out int deviceUserId)
        {
            bool result = false;

            deviceUserId = 0;

            if (deviceUser != null)
            {
                result = _userStorage.DeviceUserExists(deviceUser) ? _userStorage.UpdateDeviceUserStatus(deviceUser) : _userStorage.AddDeviceUser(deviceUser);

                if (result)
                {
                    result = _userStorage.GetDeviceUserIdByLoginName(deviceUser, out deviceUserId);
                }
            }

            return result;
        }

        private bool GetSetDeviceLocationId(IpLocation location, out int locationId)
        {
            bool result = false;

            locationId = 0;

            if (location != null)
            {
                result = _locationStorage.LocationExists(location) ? _locationStorage.UpdateLocationStatus(location) : _locationStorage.AddLocation(location);

                if (result)
                {
                    result = _locationStorage.GetLocationIdByIp(location, out locationId);
                }
            }

            return result;
        }

        private bool GetSetDeviceId(EltraDevice device, out int deviceId)
        {
            bool result = false;

            deviceId = 0;

            if (device != null)
            {
                result = _deviceStorage.DeviceExists(device) ? _deviceStorage.UpdateDeviceStatus(device, device.Status) : _deviceStorage.AddDevice(device);

                if (result)
                {
                    result = _deviceStorage.GetDeviceIdBySerialNumber(device, out deviceId);
                }
            }

            return result;
        }
        
        private bool GetSessionIdByUuid(Session session, int userId, out int id)
        {
            var result = GetSessionIdByUuid(session.Uuid, userId, out id);

            return result;
        }

        private bool GetSessionIdByUuid(string uuid, int userId, out int id)
        {
            bool result = false;

            id = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSessionIdByUuid));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));
                    command.Parameters.Add(new DbParameterWrapper("@userId", userId));
                    
                    var queryResult = command.ExecuteScalar();

                    if (!(queryResult is DBNull))
                    {
                        id = Convert.ToInt32(queryResult);
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetSessionIdByUuid", e);;
            }

            return result;
        }

        private bool GetSessionIdByUuid(string uuid, out int id)
        {
            bool result = false;

            id = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSessionIdByUuidOnly));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", uuid));

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
                MsgLogger.Exception($"{GetType().Name} - GetSessionIdByUuid", e);;
            }

            return result;
        }

        private bool SessionExists(Session session, int userId)
        {
            bool result = false;

            if (session != null)
            {
                result = GetSessionIdByUuid(session, userId, out var id) && id > 0;
            }

            return result;
        }

        private bool UpdateSessionStatus(Session session, int userId)
        {
            bool result = false;

            if (session != null)
            {
                result = UpdateSessionStatus(session.Uuid, userId, session.Status);
            }

            return result;
        }
        
        private bool UpdateSessionStatus(string uuid, int userId, SessionStatus status)
        {
            bool result = false;
            
            if (GetSessionIdByUuid(uuid, userId, out var sessionId) && sessionId > 0)
            {
                try
                {
                    string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateSessionStatusById));

                    using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                    {
                        command.Parameters.Add(new DbParameterWrapper("@session_id", sessionId));
                        command.Parameters.Add(new DbParameterWrapper("@status", (int)status));

                        var queryResult = command.ExecuteNonQuery();

                        if (queryResult > 0)
                        {
                            result = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - UpdateSessionStatus", e);;
                }
            }
            
            return result;
        }
        
        private bool AddSession(Session session, int userId, int locationId)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertSession));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@uuid", session.Uuid));
                    command.Parameters.Add(new DbParameterWrapper("@deviceUserId", userId));
                    command.Parameters.Add(new DbParameterWrapper("@locationId", locationId));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)session.Status));
                    command.Parameters.Add(new DbParameterWrapper("@timeout", session.Timeout));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddSession", e);
            }

            return result;
        }

        private bool AddSessionDevice(EltraDevice device, int sessionId, int deviceId)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertSessionDevice));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@session_idref", sessionId));
                    command.Parameters.Add(new DbParameterWrapper("@device_idref", deviceId));
                    
                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = AddCommandSet(device.CommandSet, sessionId, deviceId);

                        if(result)
                        {
                            result = AddToolSet(device.ToolSet, deviceId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AddSessionDevice", e);;
            }

            return result;
        }
        
        private bool UpdateSessionDevice(EltraDevice device, int sessionId, int deviceId)
        {
            bool result = false;

            try
            {
                result = UpdateCommandSet(device.CommandSet, sessionId, deviceId);

                if(result)
                { 
                    result = UpdateToolSet(device.ToolSet, deviceId);    
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - UpdateSessionDevice", e);;
            }

            return result;
        }
        
        private bool AddCommandSet(DeviceCommandSet deviceCommandSet, int sessionId, int deviceId)
        {
            var result = _deviceCommandSetStorage.AddCommandSet(deviceCommandSet, sessionId, deviceId);

            return result;
        }

        private bool AddToolSet(DeviceToolSet deviceToolSet, int deviceId)
        {
            var result = _deviceToolSetStorage.AddToolSet(deviceToolSet, deviceId);

            return result;
        }

        private bool UpdateCommandSet(DeviceCommandSet deviceCommandSet, int sessionId, int deviceId)
        {
            var result = _deviceCommandSetStorage.UpdateCommandSet(deviceCommandSet, sessionId, deviceId);

            return result;
        }

        private bool UpdateToolSet(DeviceToolSet deviceToolSet, int deviceId)
        {
            var result = _deviceToolSetStorage.UpdateToolSet(deviceId, deviceToolSet);

            return result;
        }

        private bool SessionDeviceExists(int sessionId, int deviceId)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectSessionDevice));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@session_idref", sessionId));
                    command.Parameters.Add(new DbParameterWrapper("@device_idref", deviceId));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult!=null && !(queryResult is DBNull))
                    {
                        var id = Convert.ToInt32(queryResult);
                        result = id > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SessionDeviceExists", e);;
            }

            return result;
        }

        #endregion
        
        private void OnConnectionStringChanged()
        {
            CloseConnection();

            Connection = CreateConnection();
        }

        private DbConnectionWrapper CreateConnection()
        {
            DbConnectionWrapper result = null;

            try
            {
                var connection = DbConnectionFactory.GetDbConnection(Engine, ConnectionString);

                if (connection != null)
                {
                    connection.Open();

                    if (connection.State == ConnectionState.Open)
                    {
                        result = connection;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CreateConnection", e);;
            }

            return result;
        }

        private bool CloseConnection()
        {
            bool result = false;

            if (Connection != null && Connection.State == ConnectionState.Open)
            {
                Connection.Close();

                result = true;
            }

            return result;
        }

        public void Dispose()
        {
            CloseConnection();
        }

        #region Lock

        public string GetDeviceOwnerSessionId(ulong serialNumber)
        {
            string result = string.Empty;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectDeviceOwnerSessionId));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)SessionStatus.Online));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult!=null && !(queryResult is DBNull))
                    {
                        result = Convert.ToString(queryResult);
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool LockDevice(ulong serialNumber, string agentUuid)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.LockDevice));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", serialNumber));
                    command.Parameters.Add(new DbParameterWrapper("@agent_uuid", agentUuid));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)SessionStatus.Online));

                    var queryResult = command.ExecuteNonQuery();

                    result = (queryResult > 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool UnlockDevice(ulong serialNumber)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextDelete(DeleteQuery.UnlockDevice));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@serial_number", serialNumber));

                    var queryResult = command.ExecuteNonQuery();

                    result = (queryResult > 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        #endregion

        #region Users

        public bool UserExists(string userName)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectUserExists));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@login_name", userName));

                    var queryResult = command.ExecuteScalar();

                    if (!(queryResult is DBNull) && Convert.ToInt32(queryResult) > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool IsUserValid(string userName, string password)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectIsUserValid));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@login_name", userName));
                    command.Parameters.Add(new DbParameterWrapper("@password", password));

                    var queryResult = command.ExecuteScalar();

                    if (!(queryResult is DBNull) && Convert.ToInt32(queryResult) > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool RegisterUser(string loginName, string userName, string password)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertDeviceUser));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@login_name", loginName));
                    command.Parameters.Add(new DbParameterWrapper("@user_name", userName));
                    command.Parameters.Add(new DbParameterWrapper("@password", password));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)UserStatus.Unlocked));

                    var queryResult = command.ExecuteNonQuery();

                    result = (queryResult > 0);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool SignInUser(string userName, string password, out string token)
        {
            bool result = false;

            token = Guid.NewGuid().ToString();

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateUserStatus));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@login_name", userName));
                    command.Parameters.Add(new DbParameterWrapper("@password", password));
                    command.Parameters.Add(new DbParameterWrapper("@token", token));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)UserStatus.SignedIn));

                    var queryResult = command.ExecuteNonQuery();

                    result = queryResult > 0;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem", e);;
            }

            return result;
        }

        public bool SignOutUser(string token)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateUserStatusByToken));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@token", token));
                    command.Parameters.Add(new DbParameterWrapper("@status", (int)UserStatus.SignedIn));
                    command.Parameters.Add(new DbParameterWrapper("@req_status", (int)UserStatus.SignedOut));

                    var queryResult = command.ExecuteNonQuery();

                    result = queryResult > 0;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("SessionStorageItem - SignOutUser", e);
            }

            return result;
        }

        #endregion

        #endregion        
    }
}

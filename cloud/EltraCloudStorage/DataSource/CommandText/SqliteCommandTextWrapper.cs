using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;

namespace EltraCloudStorage.DataSource.CommandText
{
    class SqliteCommandTextWrapper : DbCommandTextWrapper
    {
        public SqliteCommandTextWrapper(DbCommandText selection)
            : base(selection)
        {
        }

        protected override string GetSelectCommandText()
        {
            string result = string.Empty;

            switch (Query)
            {
                case SelectQuery.SelectDeviceVersionId:
                    result = "select device_version_id from device_version where hardware_version=@hardware_version and software_version=@software_version and application_number=@application_number and application_version=@application_version";
                    break;
                case SelectQuery.SelectDeviceIdBySerialNumber:
                    result = "select device_id from device where serial_number=@serial_number";
                    break;
                case SelectQuery.SelectDeviceUserIdByLoginName:
                    result = "select device_user_id from device_user where login_name=@login_name";
                    break;
                case SelectQuery.SelectSessionIdByUuid:
                    result = "select session_id from session where uuid=@uuid and device_user_idref=@userId";
                    break;
                case SelectQuery.SelectSessionIdByUuidOnly:
                    result = "select session_id from session where uuid=@uuid";
                    break;
                case SelectQuery.SelectSessionDevice:
                    result = "select session_devices_id from session_devices where session_idref=@session_idref and device_idref=@device_idref";
                    break;
                case SelectQuery.SelectParameter:
                    result = "SELECT parameter_id FROM [parameter] WHERE device_id=@device_id AND unique_id=@unique_id";
                    break;
                case SelectQuery.SelectCommand:
                    result =
                        "SELECT command_id FROM command WHERE device_idref=@device_id AND [name]=@name";
                    break;
                case SelectQuery.SelectDataType:
                    result = "SELECT data_type_id FROM data_type WHERE [type]=@type AND size_bytes=@size_bytes AND size_bits=@size_bits";
                    break;
                case SelectQuery.SelectDataTypeByTypeOnly:
                    result = "SELECT data_type_id FROM data_type WHERE [type]=@type";
                    break;
                case SelectQuery.SelectCommandParameter:
                    result =
                        "SELECT command_parameter_id FROM command_parameter WHERE command_idref=@command_id AND data_type_idref=@data_type_id AND [name]=@name";
                    break;
                case SelectQuery.SelectExecCommandId:
                    result =
                        "SELECT exec_command_id FROM exec_command WHERE command_idref=@command_id AND source_session_idref=@session_id";
                    break;
                case SelectQuery.SelectPopCommands:
                    result = "select ec.exec_command_id, c.name, s.uuid, ec.modified from command as c" + 
                                " inner join device as d on c.device_idref = d.device_id" +
                                " join exec_command as ec on ec.command_idref = c.command_id" +
                                " inner join session as s on s.session_id = ec.source_session_idref" +
                                " where d.serial_number = @serialNumber and ec.status = @status order by ec.exec_command_id desc limit 1;";
                    break;
                case SelectQuery.SelectExecParameter:
                    result = "SELECT cp.name, cp.type, dt.type as datatype, dt.size_bytes, dt.size_bits, ecp.value, ecp.modified"
                             + " FROM exec_command_parameter as ecp"
                             + " INNER JOIN command_parameter as cp"
                             + " ON ecp.command_parameter_idref = cp.command_parameter_id"
                             + " INNER JOIN data_type AS dt"
                             + " ON dt.data_type_id = cp.data_type_idref"
                             + " WHERE exec_command_idref = @exec_command_id;";
                    break;
                case SelectQuery.SelectExecCommandParameter:
                    result = "select * from exec_command_parameter WHERE exec_command_idref=@exec_command_id AND command_parameter_idref=@command_parameter_id";
                    break;
            }

            return result;
        }

        protected override string GetUpdateCommandText()
        {
            string result = string.Empty;

            switch (CommandTextUpdating)
            {
                case UpdateQuery.UpdateDeviceStatus:
                    result = "update device set [status]=@status, modified=datetime('now','localtime') where device_id=@device_id";
                    break;
                case UpdateQuery.UpdateDeviceUserStatus:
                    result = "update device_user set [status]=@status, modified=datetime('now','localtime') where device_user_id=@device_user_id";
                    break;
                case UpdateQuery.UpdateSessionStatus:
                    result = "update session set [status]=@status, modified=datetime('now','localtime') where session_id=@session_id";
                    break;
                case UpdateQuery.UpdateSessionDeviceStatus:
                    result = "UPDATE device set [status] = @status, modified=datetime('now','localtime') where serial_number=@serialNumber and device_id = " + 
                             "(SELECT device_idref FROM session_devices " + 
                             "WHERE session_idref = (SELECT session_id FROM session WHERE uuid = @uuid and device_user_idref" + 
                             "= (select device_user_id from device_user where login_name = @login)))";
                    break;
                case UpdateQuery.UpdateSessionCommand:
                    result = "UPDATE session_command SET [status]=@status, modified=datetime('now','localtime') WHERE session_idref=@session_id AND command_idref=@command_id";
                    break;
                case UpdateQuery.UpdateExecCommandStatus:
                    result = "UPDATE exec_command SET [status]=@status, modified=datetime('now','localtime') WHERE exec_command_id=@exec_command_id";
                    break;
                case UpdateQuery.UpdateExecCommandParameterValue:
                    result = "UPDATE exec_command_parameter SET [value]=@value, modified=datetime('now','localtime') WHERE exec_command_idref=@exec_command_id AND command_parameter_idref=@command_parameter_id";
                    break;
                case UpdateQuery.UpdateDataType:
                    result = "UPDATE data_type SET size_bytes=@size_bytes, size_bits=@size_bits, modified=datetime('now','localtime') WHERE data_type_id=@data_type_id";
                    break;
            }

            return result;
        }

        protected override string GetDeleteCommandText()
        {
            string result = string.Empty;

            switch (CommandTextDelete)
            {
                case DeleteQuery.DeleteUndefined: 
                    break;
                case DeleteQuery.UnlockDevice:
                    break;
            }

            return result;
        }

        protected override string GetInsertCommandText()
        {
            string result = string.Empty;

            switch (CommandTextInsertion)
            {
                case InsertQuery.InsertDeviceVersion:
                    result = "insert into device_version (hardware_version, software_version, application_version, application_number, created, modified) " +
                             "values (@hardware_version, @software_version, @application_version, @application_number, datetime('now','localtime'), datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertDevice:
                    result = "INSERT INTO device (serial_number,device_name,product_name,device_version_idref,status,created,modified) " +
                                "VALUES(@serial_number,@device_name,@product_name,@device_version_idref,@status,datetime('now','localtime'),datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertDeviceUser:
                    result = "insert into device_user(login_name, user_name, [status],created,modified) values(@login_name, @user_name, @status,datetime('now','localtime'),datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertSession:
                    result = "insert into session(uuid,device_user_idref,[status],created,modified) values(@uuid,@deviceUserId,@status,datetime('now','localtime'),datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertSessionDevice:
                    result = "INSERT INTO session_devices(session_idref, device_idref,created) VALUES(@session_idref,@device_idref,datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertParameter:
                    result = "INSERT INTO parameter(device_id,unique_id,[index],sub_index,created) VALUES(@device_id,@unique_id,@index,@sub_index, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertParameterValue:
                    result = "INSERT INTO parameter_value(parameter_idref, actual_value, created) VALUES(@parameter_id, @actual_value, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertCommand:
                    result =
                        "INSERT INTO command(device_idref,[name],created,modified) VALUES(@device_id, @name, datetime('now','localtime'), datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertExecCommand:
                    result =
                        "INSERT INTO exec_command (source_session_idref,command_idref,[status],created,modified) VALUES(@session_id, @command_id, @status, datetime('now','localtime'), datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertDataType:
                    result =
                        "INSERT INTO data_type([type],size_bytes,size_bits,created) VALUES(@type, @size_bytes, @size_bits, datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertCommandParameter:
                    result = "INSERT INTO command_parameter(command_idref,[name],[type],data_type_idref,created,modified)" +
                             " VALUES(@command_id, @name, @type, @data_type_id, datetime('now','localtime'), datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertExecCommandParameter:
                    result = "INSERT INTO exec_command_parameter(exec_command_idref,command_parameter_idref, [value],created,modified) VALUES(@exec_command_id, @command_parameter_id, @value, datetime('now','localtime'), datetime('now','localtime'))";
                    break;
                case InsertQuery.InsertSessionCommand:
                    result =
                        "INSERT INTO session_command(session_idref,command_idref,[status], created, modified) VALUES(@session_id, @command_id, @status, datetime('now','localtime'), datetime('now','localtime'))";
                    break;
            }
            
            return result;
        }
    }
}

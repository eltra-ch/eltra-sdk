/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;

namespace EltraCloudStorage.DataSource.CommandText
{
    class MySqlCommandTextWrapper : DbCommandTextWrapper
    {
        public MySqlCommandTextWrapper(DbCommandText selection)
            : base(selection)
        {
        }
        
        protected override string GetSelectCommandText()
        {
            string result = string.Empty;

            switch (Query)
            {
                case SelectQuery.SelectDeviceVersion:
                    result =
                        "select hardware_version, software_version, application_number, application_version from device_version where device_version_id=@device_version_id";
                    break;
                case SelectQuery.SelectDeviceVersionId:
                    result =
                        "select device_version_id from device_version where hardware_version=@hardware_version and software_version=@software_version and application_number=@application_number and application_version=@application_version";
                    break;
                case SelectQuery.SelectDeviceBySerialNumber:
                    result =
                        "select d.product_family, d.product_name, dv.hardware_version, dv.software_version, dv.application_version, dv.application_number, d.status, d.modified from device as d inner join device_version as dv on d.device_version_idref=dv.device_version_id where serial_number=@serial_number";
                    break;
                case SelectQuery.SelectDeviceIdBySerialNumber:
                    result = "select device_id from device where serial_number=@serial_number";
                    break;
                case SelectQuery.SelectDeviceById:
                    result =
                        "select d.serial_number, d.product_family, d.product_name, dv.hardware_version, dv.software_version, dv.application_version, dv.application_number, d.status, d.modified from device as d inner join device_version as dv on d.device_version_idref=dv.device_version_id where device_id=@device_id";
                    break;
                case SelectQuery.SelectDevicesByStatus:
                    result =
                        "select d.serial_number, d.product_family, d.product_name, dv.hardware_version, dv.software_version, dv.application_number,dv.application_version, d.status, d.modified, d.created from device as d inner join device_version as dv on d.device_version_idref=dv.device_version_id where `status`=@status";
                    break;
                case SelectQuery.SelectDevices:
                    result =
                        "select d.serial_number, d.product_family, d.product_name, dv.hardware_version, dv.software_version, dv.application_number,dv.application_version, d.status, d.modified, d.created from device as d inner join device_version as dv on d.device_version_idref=dv.device_version_id";
                    break;
                case SelectQuery.SelectDeviceUserById:
                    result =
                        "select login_name, user_name, status, modified from device_user where device_user_id=@device_user_id";
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
                    result =
                        "select session_devices_id from session_devices where session_idref=@session_idref and device_idref=@device_idref";
                    break;
                case SelectQuery.SelectParameter:
                    result = "SELECT parameter_id FROM `parameter` as p" +
                                " INNER JOIN device as d" +
                                " ON p.device_description_idref = d.device_description_idref" +
                                " WHERE d.device_id = @device_id AND p.index = @index AND p.sub_index = @sub_index";
                    break;
                case SelectQuery.GetParameterValueId:
                    result = "SELECT MAX(pv.parameter_value_id) as id FROM parameter_value AS pv" +
                                " INNER JOIN parameter AS p ON p.parameter_id = pv.parameter_idref" +
                                " INNER JOIN device as d ON d.device_id = pv.device_idref" +
                                " WHERE d.serial_number = @serial_number AND p.index = @index AND p.sub_index = @subindex;";
                    break;
                case SelectQuery.GetParameterValue:
                    result = "SELECT pv.actual_value, pv.created FROM parameter_value AS pv" +
                                " WHERE pv.parameter_value_id = @parameterId";
                    /*result = "SELECT pv1.actual_value, pv1.created FROM parameter_value AS pv1" +
                                " WHERE pv1.parameter_value_id = (SELECT id FROM(" +
                                " SELECT MAX(pv.parameter_value_id) as id FROM parameter_value AS pv" +
                                " INNER JOIN parameter AS p ON p.parameter_id = pv.parameter_idref" +
                                " INNER JOIN device as d ON d.device_id = pv.device_idref" +
                                " WHERE d.serial_number = @serial_number AND p.index = @index AND p.sub_index = @subindex) as latest_pv);";*/
                    /*result = "SET @pv_id = (" +
                                " SELECT MAX(pv.parameter_value_id) as id FROM parameter_value AS pv" +
                                " INNER JOIN parameter AS p ON p.parameter_id = pv.parameter_idref" +
                                " INNER JOIN device as d ON d.device_id = pv.device_idref" +
                                " WHERE d.serial_number = @serial_number AND p.index = @index AND p.sub_index = @subindex);" +
                                " SELECT pv1.actual_value, pv1.created FROM parameter_value AS pv1" +
                                " WHERE pv1.parameter_value_id = @pv_id;";*/
                    break;                
                case SelectQuery.SelectParameterHistory:
                    result = "select pv.actual_value, pv.created from parameter_value as pv" +
                              " inner join parameter as p" +
                              " on pv.parameter_idref = p.parameter_id" +
                              " where p.unique_id = @uniqueId" +
                              " and pv.device_idref = @deviceId and pv.created between @from and @to" +
                              " order by pv.parameter_value_id;";
                    break;
                case SelectQuery.SelectParameterPairHistory:
                    result = "select p.unique_id, pv.actual_value, pv.created from parameter_value as pv" +
                              " inner join parameter as p" +
                              " on pv.parameter_idref = p.parameter_id" +
                              " where (p.unique_id = @uniqueId1 or p.unique_id = @uniqueId2)" +
                              " and pv.device_idref = @deviceId and pv.created between @from and @to" +
                              " order by pv.parameter_value_id;";
                    break;
                case SelectQuery.SelectCommand:
                    result =
                        "SELECT command_id FROM command WHERE device_idref=@device_id AND `name`=@name";
                    break;
                case SelectQuery.SelectDataType:
                    result =
                        "SELECT data_type_id FROM data_type WHERE `type`=@type AND size_bytes=@size_bytes AND size_bits=@size_bits";
                    break;
                case SelectQuery.SelectDataTypeByTypeOnly:
                    result = "SELECT data_type_id FROM data_type WHERE `type`=@type";
                    break;
                case SelectQuery.SelectCommandParameter:
                    result =
                        "SELECT command_parameter_id FROM command_parameter WHERE command_idref=@command_id AND data_type_idref=@data_type_id AND `name`=@name";
                    break;
                case SelectQuery.SelectExecCommandId:
                    result =
                        "SELECT exec_command_id FROM exec_command" +
                        " WHERE uuid=@uuid AND command_idref=@command_id AND" + 
                        " source_session_idref=@session_id";
                    break;
                case SelectQuery.SelectPopCommands:
                    result = "select ec.uuid, ec.exec_command_id, c.name, s.uuid, ec.modified from command as c" +
                             " inner join device as d on c.device_idref = d.device_id" +
                             " join exec_command as ec on ec.command_idref = c.command_id" +
                             " inner join session as s on s.session_id = ec.source_session_idref" +
                             " where d.serial_number = @serialNumber and ec.status = @status and s.status = @session_status" +
                             " and ec.modified>=DATE_SUB(NOW(), INTERVAL @max_delay SECOND) order by ec.exec_command_id;";
                    break;
                case SelectQuery.SelectMultiPopCommands:
                    result = "select d.serial_number, ec.uuid, ec.exec_command_id, c.name, s.uuid, ec.modified from command as c" +
                             " inner join device as d on c.device_idref = d.device_id" +
                             " inner join exec_command as ec on ec.command_idref = c.command_id" +
                             " inner join session as s on s.session_id = ec.source_session_idref" +
                             " where d.serial_number IN (@serialNumberArray) and ec.status IN (@statusArray) and s.status = @session_status" +
                             " and ec.communication_status=@communication_status" +
                             " and ec.modified>=DATE_SUB(NOW(), INTERVAL @max_delay SECOND) order by d.serial_number, ec.exec_command_id;";
                    break;
                case SelectQuery.SelectExecCommands:
                    result = "select d.serial_number, ec.uuid, ec.exec_command_id, c.name, s.uuid, ec.status, ec.modified from command as c" +
                             " inner join device as d on c.device_idref = d.device_id" +
                             " inner join exec_command as ec on ec.command_idref = c.command_id" +
                             " inner join session as s on s.session_id = ec.source_session_idref" +
                             " where s.uuid=@source_session_uuid and ec.status IN (@statusArray) and s.status = @session_status" +
                             " and ec.communication_status=@communication_status" +
                             " and ec.modified>=DATE_SUB(NOW(), INTERVAL @max_delay SECOND) order by d.serial_number, ec.exec_command_id;";
                    break;
                case SelectQuery.SelectSpecificPopCommand:
                    result = "select ec.exec_command_id, c.name, s.uuid, ec.modified from command as c" +
                             " inner join device as d on c.device_idref = d.device_id" +
                             " join exec_command as ec on ec.command_idref = c.command_id" +
                             " inner join session as s on s.session_id = ec.source_session_idref" +
                             " where ec.uuid=@uuid and d.serial_number = @serialNumber" + 
                             " and ec.status = @status and s.status = @session_status and" + 
                             " ec.modified>=DATE_SUB(NOW(), INTERVAL @max_delay SECOND) order by ec.exec_command_id desc limit 1;";
                    break;
                case SelectQuery.SelectExecParameter:
                    result =
                        "SELECT cp.name, cp.type, dt.type as datatype, dt.size_bytes, dt.size_bits, ecp.value, ecp.modified"
                        + " FROM exec_command_parameter as ecp"
                        + " INNER JOIN command_parameter as cp"
                        + " ON ecp.command_parameter_idref = cp.command_parameter_id"
                        + " INNER JOIN data_type AS dt"
                        + " ON dt.data_type_id = cp.data_type_idref"
                        + " WHERE exec_command_idref = @exec_command_id;";
                    break;
                case SelectQuery.SelectExecCommandParameter:
                    result =
                        "select * from exec_command_parameter WHERE exec_command_idref=@exec_command_id AND command_parameter_idref=@command_parameter_id";
                    break;
                case SelectQuery.SelectSessionOlderThanMin:
                    result = "SELECT uuid, modified from session WHERE modified<=DATE_SUB(NOW(), INTERVAL @minutes MINUTE) AND status=@status";
                    break;
                case SelectQuery.SelectSessionsByStatus:
                    result = "select s.uuid, s.modified, s.created, s.timeout, du.login_name, du.user_name, du.password, du.status, du.modified, du.created, l.ip,l.country_code,l.country,l.region,l.city,l.latitude,l.longitude" + 
                                " from session as s" +
                                " inner join device_user as du on du.device_user_id=s.device_user_idref" +
                                " inner join location as l on s.location_idref=l.location_id" +                                
                                " where s.status=@status";
                    break;
                case SelectQuery.SelectDeviceSessionsByStatus:
                    result = "select s.uuid, s.modified, s.created, s.timeout, du.login_name, du.user_name, du.password, du.status, du.modified, du.created, l.ip,l.country_code,l.country,l.region,l.city,l.latitude,l.longitude" +
                                " from session as s" +
                                " inner join device_user as du on du.device_user_id=s.device_user_idref" +
                                " inner join location as l on s.location_idref=l.location_id" +
                                " left join session_devices as sd on sd.session_idref = s.session_id" +
                                " where s.status=@status and sd.session_idref is not null";
                    break;
                case SelectQuery.SelectSessionsByUserPassword:
                    result = "select s.uuid, s.modified, s.created, du.user_name, du.status, du.modified, du.created, l.ip,l.country_code,l.country,l.region,l.city,l.latitude,l.longitude" +
                             " from session as s" +
                             " inner join device_user as du on du.device_user_id=s.device_user_idref" +
                             " inner join location as l on s.location_idref=l.location_id" +
                             " where s.status=@status and du.login_name=@login and du.password=@password";
                    break;
                case SelectQuery.GetSessionLocation:
                    result = "select" +
                             " l.ip,l.country_code,l.country,l.region,l.city,l.latitude,l.longitude" +
                             " from session as s " +
                             " inner join location as l on s.location_idref=l.location_id" +
                             " where s.uuid=@uuid";
                    break;
                case SelectQuery.SelectSessionByUuid:
                    result = "select s.uuid, s.status, s.modified, s.created, du.login_name, du.user_name, du.password, du.status, du.modified, du.created," + 
                             " l.ip,l.country_code,l.country,l.region,l.city,l.latitude,l.longitude" +
                             " from session as s inner join device_user as du on du.device_user_id=s.device_user_idref" + 
                             " inner join location as l on s.location_idref=l.location_id" +
                             " where s.uuid=@uuid";
                    break;
                case SelectQuery.SelectSessionBySerialNumber:
                    result = "select s.uuid, s.status, s.modified, s.created, du.login_name, du.user_name, du.password, du.status, du.modified, du.created," +
                             " l.ip,l.country_code,l.country,l.region,l.city,l.latitude,l.longitude" +
                             " from session as s inner join device_user as du on du.device_user_id=s.device_user_idref" +
                             " inner join location as l on s.location_idref=l.location_id" +
                             " inner join session_devices as sd on sd.session_idref = s.session_id" +
                             " inner join device as d on d.device_id = sd.device_idref" +
                             " where d.serial_number = @serialNumber and s.status = @status order by s.modified desc";
                    break;
                case SelectQuery.SelectDevicebySerialNumber:
                    result = "select s.uuid, d.device_id, d.product_family, d.serial_number, d.status, d.modified, d.created, dv.hardware_version, dv.software_version, dv.application_number, dv.application_version from session_devices as sd"
                            + " inner join session as s on s.session_id = sd.session_idref"
                            + " inner join device as d on d.device_id = sd.device_idref"
                            + " inner join device_version as dv on d.device_version_idref = dv.device_version_id"
                            + " where d.serial_number = @serial_number and s.status = @status";
                    break;
                case SelectQuery.SelectDevicesbySessionId:
                    result = "select d.device_id, d.product_family, d.serial_number, d.status, d.modified, d.created, dv.hardware_version, dv.software_version, dv.application_number, dv.application_version from session_devices as sd"
                             + " inner join session as s on s.session_id = sd.session_idref"
                             + " inner join device as d on d.device_id = sd.device_idref"
                             + " inner join device_version as dv on d.device_version_idref = dv.device_version_id"
                             + " where s.status = @status and s.uuid = @uuid group by d.serial_number";
                    break;
                case SelectQuery.SelectDeviceCountbySessionId:
                    result = "select count(*) from session_devices as sd"
                             + " inner join session as s on s.session_id = sd.session_idref"
                             + " inner join device as d on d.device_id = sd.device_idref"
                             + " inner join device_version as dv on d.device_version_idref = dv.device_version_id"
                             + " where s.status = @status and s.uuid = @uuid group by d.serial_number";
                    break;
                case SelectQuery.SelectCommandStatus:
                    result = "SELECT s.uuid, ec.status, ec.modified FROM exec_command as ec" +
                                " inner join command as c on c.command_id = ec.command_idref" +
                                " inner join device as d on d.device_id = c.device_idref" +
                                " inner join session as s on s.session_id = ec.source_session_idref" +
                                " where ec.uuid=@uuid and d.serial_number = @serial_number and " + 
                                " c.name = @command_name and s.uuid=@session_uuid order by ec.exec_command_id desc";
                    break;
                case SelectQuery.SelectLocationByIp:
                    result = "SELECT country_code, country, region, city, latitude, longitude, created, modified FROM location where ip = @ip";
                    break;
                case SelectQuery.SelectLocationIdByIp:
                    result = "SELECT location_id FROM location where ip = @ip";
                    break;
                case SelectQuery.SelectDeviceOwnerSessionId:
                    result = "select s.uuid from device_lock as dl" +
                                " inner join device as d on dl.device_idref = d.device_id" +
                                " inner join session as s on dl.session_idref = s.session_id" +
                                " where s.status = @status and d.serial_number = @serial_number" + 
                                " order by dl.device_lock_id desc limit 1";
                    break;
                case SelectQuery.SelectUserExists:
                {
                    result = "select device_user_id from device_user where login_name=@login_name";
                } break;
                case SelectQuery.SelectIsUserValid:
                {
                    result = "select device_user_id from device_user where login_name=@login_name and password=@password";
                } break;
                case SelectQuery.SelectToolByUuid:
                    result =
                        "SELECT device_tool_id FROM device_tool WHERE uuid=@uuid";
                    break;
                case SelectQuery.IsDeviceUsedByAgent:
                {
                    result = "select sl.session_link_id from session_link as sl" +
                                " inner join session as ms on ms.session_id = sl.master_session_idref" +
                                " inner join session as ss on ss.session_id = sl.slave_session_idref" +
                                " inner join session_devices as sd on sd.session_idref = sl.master_session_idref" +
                                " inner join device as d on d.device_id = sd.device_idref" +
                                " where ss.uuid = @session_id and d.serial_number = @serial_number and ms.status = @status and ss.status = @status" +
                                " order by sl.session_link_id limit 1;";

                } break;
                case SelectQuery.SelectSessionCommandIds:
                    {
                        result = "select session_id, command_id from session as s" +
                                    " inner join device as d on d.serial_number = @serial_number" +
                                    " inner join command as c on c.device_idref = d.device_id" +
                                    " where c.name = @command_name and s.uuid = @session_uuid";
                    }
                    break;
                case SelectQuery.GetDeviceDescription:
                    {
                        result = "select dd.device_description_id, dd.content, dd.encoding, dd.hash_code, dd.modified from device_description as dd" +
                                    " where dd.device_version_idref = @versionId;";
                    }
                    break;
                case SelectQuery.GetDeviceDescriptionIdByVersionId:
                    {
                        result = "select dd.device_description_id from device_description as dd" +
                                    " where dd.device_version_idref = @versionId;";
                    }
                    break;
                case SelectQuery.GetDeviceDescriptionIdByDeviceId:
                    {
                        result = "select d.device_description_idref from device as d" +
                                    " where d.device_id = @device_id;";
                    }
                    break;
                case SelectQuery.DeviceDescriptionExist:
                    {
                        result = "select dd.device_description_id from device_description as dd" +
                                    " where dd.hash_code = @hash_code;";
                    }
                    break;
                case SelectQuery.DeviceDescriptionChanged:
                    {
                        result = "select dd.device_description_id from device_description as dd" +
                                    " where dd.device_version_idref = @versionId and dd.hash_code <> @hash_code;";
                    }
                    break;
                case SelectQuery.GetDeviceVersionId:
                    {
                        result = "select d.device_version_idref from device as d" +
                                    " inner join session_devices as sd on sd.device_idref = d.device_id" +
                                    " inner join session as s on s.session_id = sd.session_idref" +
                                    " where s.uuid = @uuid;";
                    } break;
                case SelectQuery.GetDeviceTools:
                    {
                        result = "select dt.name, dt.uuid from device_tool as dt" +
                                    " inner join device_tool_set as dts" +
                                    " on dt.device_tool_id = dts.device_tool_idref" +
                                    " where dts.device_idref = @device_id and dt.status = @status group by device_tool_id";
                    } break;
                case SelectQuery.GetDeviceCommands:
                    {
                        result = "SELECT c.command_id, c.name, cp.name, cp.type as direction, dt.type, dt.size_bytes FROM command as c" + 
                                    " inner join command_parameter as cp on cp.command_idref = c.command_id" +
                                    " inner join data_type as dt on dt.data_type_id = cp.data_type_idref" +
                                    " where c.device_idref = @device_id group by cp.command_parameter_id;";
                    } break;
                case SelectQuery.GetSessionLink:
                    {
                        result = "SELECT session_link_id FROM session_link WHERE master_session_idref=@master_session_id AND slave_session_idref=@slave_session_id";
                    }
                    break;
                case SelectQuery.GetLinkedMasterSessionUuids:
                    {
                        result = "SELECT sm.uuid FROM session_link as sl" +
                                " INNER JOIN session as sm ON sm.session_id = sl.master_session_idref" +
                                " INNER JOIN session as ss ON ss.session_id = sl.slave_session_idref" +
                                " where sm.uuid = @uuid and ss.status = @status;";
                    }
                    break;
                case SelectQuery.GetLinkedSlaveSessionUuids:
                    {
                        result = "SELECT ss.uuid FROM session_link as sl" +
                                " INNER JOIN session as sm ON sm.session_id = sl.master_session_idref" +
                                " INNER JOIN session as ss ON ss.session_id = sl.slave_session_idref" +
                                " where ss.uuid = @uuid and sm.status = @status;";
                    }
                    break;
                case SelectQuery.GetSessionStatus:
                    {
                        result = "select s.status from session as s where s.uuid=@uuid";
                    }
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
                    result = "update device set `status`=@status, modified=NOW() where device_id=@device_id";
                    break;
                case UpdateQuery.UpdateDeviceUserStatus:
                    result = "update device_user set `status`=@status, modified=NOW() where device_user_id=@device_user_id";
                    break;
                case UpdateQuery.UpdateUserStatus:
                    result = "update device_user set status=@status, token=@token, modified=NOW() where login_name=@login_name and password=@password";
                    break;
                case UpdateQuery.UpdateUserStatusByToken:
                    result = "update device_user set status=@req_status, token='', modified=NOW() where token=@token and status=@status";
                    break;
                case UpdateQuery.UpdateSessionStatus:
                    result = "update session set `status`=@status, modified=NOW() where uuid=@uuid";
                    break;
                case UpdateQuery.UpdateSessionStatusById:
                    result = "update session set `status`=@status, location_idref=@location_id, modified=NOW() where session_id=@session_id";
                    break;
                case UpdateQuery.UpdateSessionDeviceStatus:
                    result = "UPDATE device set `status` = @status, modified=NOW() where serial_number=@serialNumber and device_id = " +
                             "(SELECT device_idref FROM session_devices " +
                             "WHERE session_idref = (SELECT session_id FROM session WHERE uuid = @uuid and device_user_idref" +
                             "= (select device_user_id from device_user where login_name = @login)))";
                    break;
                case UpdateQuery.UpdateSessionCommand:
                    result = "UPDATE session_command SET `status`=@status, modified=NOW() WHERE session_idref=@session_id AND command_idref=@command_id";
                    break;
                case UpdateQuery.UpdateExecCommandStatus:
                    result = "UPDATE exec_command SET `status`=@status, modified=NOW() WHERE exec_command_id=@exec_command_id";
                    break;
                case UpdateQuery.UpdateExecCommandCommStatus:
                    result = "UPDATE exec_command SET `communication_status`=@status, modified=NOW() WHERE exec_command_id=@exec_command_id";
                    break;
                case UpdateQuery.UpdateExecCommandParameterValue:
                    result = "UPDATE exec_command_parameter SET `value`=@value, modified=NOW() WHERE exec_command_idref=@exec_command_id AND command_parameter_idref=@command_parameter_id";
                    break;
                case UpdateQuery.UpdateDataType:
                    result = "UPDATE data_type SET size_bytes=@size_bytes, size_bits=@size_bits, modified=NOW() WHERE data_type_id=@data_type_id";
                    break;
                case UpdateQuery.UpdateLocationStatus:
                    result = "UPDATE location SET modified=NOW() WHERE location_id=@location_id";
                    break;
                case UpdateQuery.UpdateTool:
                    result = "UPDATE device_tool SET status=@status, modified=NOW() WHERE device_tool_id=@tool_id";
                    break;
                case UpdateQuery.UpdateToolSet:
                    result = "UPDATE device_tool_set SET modified=NOW() WHERE device_idref=@device_id AND device_tool_idref=@tool_id";
                    break;
                case UpdateQuery.UpdateDeviceDescription:
                    result = "update device_description set content=@content,encoding=@encoding,hash_code=@hash_code,modified=NOW() WHERE device_description_id=@device_description_id";
                    break;
                case UpdateQuery.SetSessionLinkStatus:
                    result = "update session_link set status=@status,modified=NOW() where session_link_id=@session_link_id";
                    break;
                case UpdateQuery.SetSessionLinkStatusBySessionId:
                    result = "update session_link as sl" +
                                " inner join session as sm on sm.session_id = sl.master_session_idref" +
                                " inner join session as ss on ss.session_id = sl.slave_session_idref" +
                                " set sl.status = @status, sl.modified = NOW()" +
                                " where sm.uuid = @uuid or ss.uuid = @uuid;";
                    break;
                case UpdateQuery.UpdateDeviceDescriptionReference:
                    {
                        result = "update device set device_description_idref=@device_description_id, modified=NOW() where serial_number=@serial_number";
                    }
                    break;
            }

            return result;
        }

        protected override string GetDeleteCommandText()
        {
            string result = string.Empty;

            switch (CommandTextDelete)
            {
                case DeleteQuery.DeleteDevice:
                    result = "delete from device where device_id=@device_id";
                    break;
                case DeleteQuery.DeleteDeviceVersion:
                    result = "delete from device_version where device_version_id=@device_version_id";
                    break;
                case DeleteQuery.UnlockDevice:
                    result = "delete from device_lock where device_idref=(select device_id from device where serial_number=@serial_number)";
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
                             "values (@hardware_version, @software_version, @application_version, @application_number, NOW(), NOW())";
                    break;
                case InsertQuery.InsertDevice:
                    result = "INSERT INTO device (serial_number,product_family,product_name,device_version_idref,device_description_idref,status,created,modified) " +
                                "VALUES(@serial_number,@product_family,@product_name,@device_version_idref,@device_description_idref,@status,NOW(),NOW())";
                    break;
                case InsertQuery.InsertDeviceUser:
                    result = "insert into device_user(login_name, user_name, password, `status`,created,modified) values(@login_name, @user_name, @password, @status,NOW(),NOW())";
                    break;
                case InsertQuery.InsertSession:
                    result = "insert into session(uuid,device_user_idref,location_idref,`status`,`timeout`,created,modified) values(@uuid,@deviceUserId,@locationId,@status,@timeout,NOW(),NOW())";
                    break;
                case InsertQuery.InsertSessionLink:
                    result = "insert into session_link(master_session_idref,slave_session_idref,status,modified,created) values(@master_session_id,@slave_session_id,@status, NOW(),NOW())";
                    break;
                case InsertQuery.InsertLocation:
                    result = "insert into location(ip,country_code,country,region,city,latitude,longitude,created,modified) values(@ip,@country_code,@country,@region,@city,@latitude,@longitude,NOW(),NOW())";
                    break;

                case InsertQuery.InsertSessionDevice:
                    result = "INSERT INTO session_devices(session_idref, device_idref,created) VALUES(@session_idref,@device_idref,NOW())";
                    break;
                case InsertQuery.InsertParameter:
                    result = "INSERT INTO parameter(device_description_idref,unique_id,`index`,sub_index,data_type_idref,created) VALUES(@device_description_idref,@unique_id,@index,@sub_index, @data_type_id, NOW())";
                    break;
                case InsertQuery.InsertParameterValue:
                    result = "INSERT INTO parameter_value(parameter_idref, device_idref, actual_value, created) VALUES(@parameter_id, @device_id, @actual_value, NOW())";
                    break;
                case InsertQuery.InsertCommand:
                    result =
                        "INSERT INTO command(device_idref,`name`,created,modified) VALUES(@device_id, @name, NOW(), NOW())";
                    break;
                case InsertQuery.InsertTool:
                    result =
                        "INSERT INTO device_tool(uuid,`name`,status,created,modified) VALUES(@uuid, @name, @status, NOW(), NOW())";
                    break;
                case InsertQuery.InsertExecCommand:
                    result =
                        "INSERT INTO exec_command (uuid, source_session_idref,command_idref,`status`,created,modified) VALUES(@uuid, @session_id, @command_id, @status, NOW(), NOW())";
                    break;
                case InsertQuery.InsertDataType:
                    result =
                        "INSERT INTO data_type(`type`,size_bytes,size_bits,created) VALUES(@type, @size_bytes, @size_bits, NOW())";
                    break;
                case InsertQuery.InsertCommandParameter:
                    result = "INSERT INTO command_parameter(command_idref,`name`,`type`,data_type_idref,created,modified)" +
                             " VALUES(@command_id, @name, @type, @data_type_id, NOW(), NOW())";
                    break;
                case InsertQuery.InsertExecCommandParameter:
                    result = "INSERT INTO exec_command_parameter(exec_command_idref,command_parameter_idref, `value`,created,modified) VALUES(@exec_command_id, @command_parameter_id, @value, NOW(), NOW())";
                    break;
                case InsertQuery.InsertSessionCommand:
                    result =
                        "INSERT INTO session_command(session_idref,command_idref,`status`, created, modified) VALUES(@session_id, @command_id, @status, NOW(), NOW())";
                    break;
                case InsertQuery.LockDevice:
                    result = "insert into device_lock(session_idref,device_idref) values(" +
                                " (select session_id from session as s where s.uuid = @agent_uuid and s.status = @status)," +
                                " (select device_id from device as d where d.serial_number = @serial_number)); ";
                    break;
                case InsertQuery.InsertDeviceToolSet:
                    result = "INSERT INTO device_tool_set(device_idref, device_tool_idref,created,modified) values(@device_id, @tool_id, NOW(),NOW())";
                    break;
                case InsertQuery.AddDeviceDescription:
                {
                    result = "insert into device_description(device_version_idref, content, encoding, hash_code, modified, created) values(@device_version_id,@content,@encoding,@hash_code,NOW(),NOW())";                  
                } break;
            }

            return result;
        }
    }
}

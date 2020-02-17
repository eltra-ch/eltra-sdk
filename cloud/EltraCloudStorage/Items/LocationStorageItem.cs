using System;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudStorage.DataSource.Command.Factory;
using EltraCloudStorage.DataSource.CommandText.Database;
using EltraCloudStorage.DataSource.CommandText.Database.Definitions;
using EltraCloudStorage.DataSource.CommandText.Factory;
using EltraCloudStorage.DataSource.Parameter;

namespace EltraCloudStorage.Items
{
    class LocationStorageItem : StorageItem
    {
        public bool LocationExists(IpLocation location)
        {
            var result = GetLocationIdByIp(location, out var id) && id > 0;

            return result;
        }

        public bool UpdateLocationStatus(IpLocation location)
        {
            bool result = false;

            if (GetLocationIdByIp(location, out var id) && id > 0)
            {
                try
                {
                    string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextUpdate(UpdateQuery.UpdateLocationStatus));

                    using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                    {
                        command.Parameters.Add(new DbParameterWrapper("@location_id", id));
                        command.Parameters.Add(new DbParameterWrapper("@modified", DateTime.Now));

                        var queryResult = command.ExecuteNonQuery();

                        if (queryResult > 0)
                        {
                            result = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception("LocationStorageItem - UpdateLocationStatus", e);
                }
            }

            return result;
        }

        public bool AddLocation(IpLocation location)
        {
            bool result = false;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextInsert(InsertQuery.InsertLocation));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@ip", location.Ip));
                    command.Parameters.Add(new DbParameterWrapper("@country_code", location.CountryCode));
                    command.Parameters.Add(new DbParameterWrapper("@country", location.Country));
                    command.Parameters.Add(new DbParameterWrapper("@region", location.Region));
                    command.Parameters.Add(new DbParameterWrapper("@city", location.City));
                    command.Parameters.Add(new DbParameterWrapper("@latitude", location.Latitude));
                    command.Parameters.Add(new DbParameterWrapper("@longitude", location.Longitude));

                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("LocationStorageItem - AddLocation", e);
            }

            return result;
        }

        public bool GetLocationIdByIp(IpLocation location, out int locationId)
        {
            bool result = false;

            locationId = 0;

            try
            {
                string commandText = DbCommandTextFactory.GetCommandText(Engine, new DbCommandTextSelect(SelectQuery.SelectLocationIdByIp));

                using (var command = DbCommandFactory.GetCommand(Engine, commandText, Connection))
                {
                    command.Parameters.Add(new DbParameterWrapper("@ip", location.Ip));

                    var queryResult = command.ExecuteScalar();

                    if (queryResult != null && !(queryResult is DBNull))
                    {
                        locationId = Convert.ToInt32(queryResult);

                        result = locationId > 0;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("LocationStorageItem - GetLocationIdByIp", e);
            }

            return result;
        }
    }
}

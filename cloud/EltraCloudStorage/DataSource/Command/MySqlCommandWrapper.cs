/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Linq;
using EltraCommon.Logger;
using EltraCloudStorage.DataSource.Command;
using EltraCloudStorage.DataSource.Connection;
using EltraCloudStorage.DataSource.Reader;
using EltraCloudStorage.Helpers;
using MySql.Data.MySqlClient;

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities

namespace EltraCloudContracts.Database.Command
{
    class MySqlCommandWrapper : DbCommandWrapper
    {
        #region Private fields

        private readonly MySqlCommand _command;

        #endregion

        #region Constructors

        public MySqlCommandWrapper(string commandText, DbConnectionWrapper connection) : base(commandText, connection)
        {
            FixMySqlUtf8 = true;
            MaxRetryOnFailure = 0;
            RetryWaitTime = 100;

            if (connection is MySqlConnectionWrapper connectionWrapper)
            {
                _command = new MySqlCommand(commandText, connectionWrapper.Connection);
            }
        }

        #endregion

        #region Methods

        #region Properties

        public int MaxRetryOnFailure { get;set; }
        public int RetryWaitTime { get; set; }
        public bool FixMySqlUtf8 { get; set; }

        #endregion

        #region Binding

        protected override void BindParameters()
        {
            if (_command != null)
            {
                _command.Parameters.Clear();

                foreach (var parameter in Parameters)
                {
                    if (FixMySqlUtf8)
                    {
                        if (parameter.Value is string mysqlLimitedUtf8Text)
                        {
                            //fix issue with limited utf8 support
                            mysqlLimitedUtf8Text =
                                new string(mysqlLimitedUtf8Text.Where(x => !char.IsSurrogate(x)).ToArray());

                            _command.Parameters.Add(new MySqlParameter(parameter.ParameterName, mysqlLimitedUtf8Text));
                        }
                        else
                        {
                            _command.Parameters.Add(new MySqlParameter(parameter.ParameterName, parameter.Value));
                        }
                    }
                    else
                    {
                        _command.Parameters.Add(new MySqlParameter(parameter.ParameterName, parameter.Value));
                    }
                }
            }
        }

        #endregion

        public override int ExecuteNonQuery()
        {
            bool commandResult = false;
            int result = -1;
            int retry = 0;
            string query = string.Empty;
            var loopHelper = new LoopHelper { SleepTime = RetryWaitTime };

            do
            {
                try
                {
                    if (_command != null)
                    {
                        CheckConnection();

                        if (Connection.State == System.Data.ConnectionState.Open)
                        {
                            BindParameters();

                            query = _command.CommandText;

                            result = _command.ExecuteNonQuery();

                            commandResult = true;
                        }
                    }
                }
                catch (MySqlException e)
                {
                    MsgLogger.Exception("MySqlCommandWrapper - ExecuteNonQuery", e, retry);
                    
                    commandResult = false;
                    
                    if (retry + 1 > MaxRetryOnFailure)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception("MySqlCommandWrapper - ExecuteNonQuery", e, retry);
                    
                    commandResult = false;
                    
                    if (retry + 1 > MaxRetryOnFailure)
                    {
                        throw;
                    }
                }

                if (!commandResult)
                {
                    loopHelper.Wait(true);
                    retry++;
                }
            }
            while (!commandResult && retry < MaxRetryOnFailure);

            return result;
        }

        public override object ExecuteScalar()
        {
            bool commandResult = false;
            object result = null;
            int retry = 0;
            string query = string.Empty;
            var loopHelper = new LoopHelper { SleepTime = RetryWaitTime };

            do
            {
                try
                {
                    if (_command != null)
                    {
                        CheckConnection();

                        if (Connection.State == System.Data.ConnectionState.Open)
                        {
                            BindParameters();

                            query = _command.CommandText;

                            result = _command.ExecuteScalar();

                            commandResult = true;
                        }
                    }
                }
                catch (MySqlException e)
                {
                    commandResult = false;

                    MsgLogger.Exception("MySqlCommandWrapper - ExecuteNonQuery", e, retry);

                    if (retry + 1 > MaxRetryOnFailure)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    commandResult = false;
                    
                    MsgLogger.Exception("MySqlCommandWrapper - ExecuteNonQuery", e, retry);

                    if (retry + 1 > MaxRetryOnFailure)
                    {
                        throw;
                    }
                }

                if (!commandResult)
                {
                    loopHelper.Wait(true);
                    retry++;
                }
            }
            while (!commandResult && retry < MaxRetryOnFailure);

            return result;
        }
        
        public override DbReaderWrapper ExecuteReader()
        {
            MySqlReaderWrapper result = null;
            int retry = 0;
            bool commandResult = false;
            string query = string.Empty;
            var loopHelper = new LoopHelper { SleepTime = RetryWaitTime };

            do
            {
                try
                {
                    if (_command != null)
                    {
                        CheckConnection();

                        if (Connection.State == System.Data.ConnectionState.Open)
                        {
                            BindParameters();

                            query = _command.CommandText;

                            result = new MySqlReaderWrapper { Wrapper = _command.ExecuteReader() };
                            
                            commandResult = true;
                        }
                    }
                }
                catch (MySqlException e)
                {
                    MsgLogger.Exception("MySqlCommandWrapper - ExecuteReader", e, retry);
                    
                    commandResult = false;

                    if (retry + 1 > MaxRetryOnFailure)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception("MySqlCommandWrapper - ExecuteReader", e, retry);
                    
                    commandResult = false;
                    
                    if (retry + 1 > MaxRetryOnFailure)
                    {
                        throw;
                    }
                }

                if (!commandResult)
                {
                    loopHelper.Wait(true);
                    retry++;
                }
            }
            while (!commandResult && retry < MaxRetryOnFailure);

            return result;
        }

        #region Dispose

        public override void Dispose()
        {
            _command?.Dispose();
        }

        #endregion

        #endregion
    }
}

#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
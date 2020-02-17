/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Data.SQLite;
using EltraCommon.Logger;
using EltraCloudStorage.DataSource.Connection;
using EltraCloudStorage.DataSource.Reader;

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities

namespace EltraCloudStorage.DataSource.Command
{
    class SqliteCommandWrapper : DbCommandWrapper
    {
    #region Private fields

        private readonly SQLiteCommand _command;

    #endregion

    #region Constructors

        public SqliteCommandWrapper(string commandText, DbConnectionWrapper connection) : base(commandText, connection)
        {
            if (connection is SqliteConnectionWrapper sqLiteConnectionWrapper)
                _command = new SQLiteCommand(commandText, sqLiteConnectionWrapper.SQLiteConnection);
        }

    #endregion

        public override int ExecuteNonQuery()
        {
            int result = -1;

            if (_command != null)
            {
                try
                {
                    CheckConnection();

                    if (Connection.State == System.Data.ConnectionState.Open)
                    {
                        BindParameters();

                        result = _command.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException e)
                {
                    MsgLogger.Exception("ExecuteNonQuery", e);
                }
                catch (Exception e)
                {
                    MsgLogger.Exception("ExecuteNonQuery", e);
                }
            }

            return result;
        }

        public override object ExecuteScalar()
        {
            object result = null;

            try
            {
                if (_command != null)
                {
                    CheckConnection();

                    if (Connection.State == System.Data.ConnectionState.Open)
                    {
                        BindParameters();

                        result = _command.ExecuteScalar();
                    }
                }
            }
            catch (SQLiteException e)
            {
                MsgLogger.Exception("ExecuteScalar", e);
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecuteScalar", e);
            }

            return result;
        }

        protected override void BindParameters()
        {
            if (_command != null)
            {
                _command.Parameters.Clear();

                foreach (var parameter in Parameters)
                {
                    _command.Parameters.Add(new SQLiteParameter(parameter.ParameterName, parameter.Value));
                }
            }
        }

        public override DbReaderWrapper ExecuteReader()
        {
            SqliteReaderWrapper result = null;
            
            try
            {
                if (_command != null)
                {
                    CheckConnection();

                    if (Connection.State == System.Data.ConnectionState.Open)
                    {
                        BindParameters();

                        result = new SqliteReaderWrapper { Wrapper = _command.ExecuteReader() };
                    }
                }
            }
            catch (SQLiteException e)
            {
                MsgLogger.Exception("ExecuteReader", e);
            }
            catch (Exception e)
            {
                MsgLogger.Exception("ExecuteReader", e);
            }

            return result;
        }

        public override void Dispose()
        {
            _command?.Dispose();
        }
    }
}

#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
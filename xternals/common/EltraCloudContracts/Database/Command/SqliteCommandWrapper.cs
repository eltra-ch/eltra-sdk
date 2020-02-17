/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
#if SQLITE
using System.Data.SQLite;
#endif
using TanamiBot.Database.Connection;
using TanamiBot.Database.Reader;

namespace TanamiBot.Database.Command
{
#if SQLITE

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
                    Logger.MessageLogger.Exception(GetType().Name, e);
                }
                catch (Exception e)
                {
                    Logger.MessageLogger.Exception(GetType().Name, e);
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
                Logger.MessageLogger.Exception(GetType().Name, e);
            }
            catch (Exception e)
            {
                Logger.MessageLogger.Exception(GetType().Name, e);
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
                Logger.MessageLogger.Exception(GetType().Name, e);
            }
            catch (Exception e)
            {
                Logger.MessageLogger.Exception(GetType().Name, e);
            }

            return result;
        }

        public override void Dispose()
        {
            _command?.Dispose();
        }
    }
#endif
}

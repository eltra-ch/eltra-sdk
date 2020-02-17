/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Data;

using MySql.Data.MySqlClient;
using TanamiBot.Database.Transaction;
using TanamiBot.Helpers;

namespace TanamiBot.Database.Connection
{
    class MySqlConnectionWrapper : DbConnectionWrapper
    {
        #region Private fields

        private MySqlConnection _connection;
        private readonly string _connectionString;

        #endregion

        #region Constructors

        public MySqlConnectionWrapper(string connectionString)
        {
            _connectionString = connectionString;

            _connection = new MySqlConnection(connectionString);
        }

        public MySqlConnectionWrapper()
        {
            _connection = new MySqlConnection();
        }

        #endregion

        #region Properties

        public override string Engine => "mysql";

        public override ConnectionState State
        {
            get
            {
                ConnectionState result = ConnectionState.Closed;

                if (_connection != null)
                {
                    result = _connection.State;
                }

                return result;
            }
        }
        
        public override string ConnectionString
        {
            get
            {
                string result = string.Empty;

                if (_connection != null)
                {
                    result = _connection.ConnectionString;
                }

                return result;
            }

            set
            {
                if (_connection != null && string.IsNullOrEmpty(_connection.ConnectionString))
                {
                    _connection.ConnectionString = value;
                }
            }
        }

        public MySqlConnection Connection => _connection;

        #endregion

        #region Methods
        
        public override void Open()
        {
            if (_connection != null && _connection.State != ConnectionState.Open)
            {
                ConnectionString = _connectionString;

                _connection?.Open();
            }
        }

        public override void Close()
        {
            _connection?.Close();
        }

        public override void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        public override DbTransactionWrapper BeginTransaction()
        {
            int retry = 0;
            bool commandResult;
            MySqlTransactionWrapper result = null;
            var loopHelper = new LoopHelper { SleepTime = RetryWaitTime };

            do
            {
                try
                {
                    var wrapper = new MySqlTransactionWrapper { Transaction = _connection?.BeginTransaction() };

                    result = wrapper;

                    commandResult = true;
                }
                catch (MySqlException e)
                {
                    Logger.MessageLogger.Exception($"source='BeginTransaction', method='{GetType().Name}'", e, retry);
                    commandResult = false;
                }
                catch (Exception e)
                {
                    Logger.MessageLogger.Exception($"source='BeginTransaction', method='{GetType().Name}'", e, retry);
                    commandResult = false;
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
        
        #endregion
    }
}

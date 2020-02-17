/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Data;
using System.Data.SQLite;
using EltraCommon.Logger;
using EltraCloudStorage.DataSource.Transaction;
using EltraCloudStorage.Helpers;

namespace EltraCloudStorage.DataSource.Connection
{
    class SqliteConnectionWrapper : DbConnectionWrapper
    {
    #region Private fields

        private readonly SQLiteConnection _sqliteConnection;
        private readonly string _connectionString;
        private static int _referenceCounter;

    #endregion

    #region Constructors

        public SqliteConnectionWrapper(string connectionString)
        {
            _connectionString = connectionString;

            _sqliteConnection = new SQLiteConnection(connectionString);
        }

        public SqliteConnectionWrapper()
        {
            _sqliteConnection = new SQLiteConnection();
        }

    #endregion

    #region Properties

        public override ConnectionState State
        {
            get
            {
                ConnectionState result = ConnectionState.Closed;

                if (_sqliteConnection != null)
                {
                    result = _sqliteConnection.State;
                }

                return result;
            }
        }

        public override string Engine => "sqlite";

        public override string ConnectionString
        {
            get
            {
                string result = string.Empty;

                if (_sqliteConnection != null)
                {
                    result = _sqliteConnection.ConnectionString;
                }

                return result;
            }

            set
            {
                if (_sqliteConnection != null && _sqliteConnection.State != ConnectionState.Open)
                {
                    _sqliteConnection.ConnectionString = value;
                }
            }
        }

        public SQLiteConnection SQLiteConnection => _sqliteConnection;

    #endregion

    #region Methods

        public override void Open()
        {
            ConnectionString = _connectionString;
            _referenceCounter++;

            if (_sqliteConnection.State == ConnectionState.Closed)
            {
                _sqliteConnection?.Open();
            }
        }

        public override void Close()
        {
            if (_sqliteConnection != null && _sqliteConnection.State == ConnectionState.Open)
            {
                _referenceCounter--;

                if (_referenceCounter == 0)
                {
                    _sqliteConnection.Close();
                }
            }
        }

        public override void Dispose()
        {
            if (_referenceCounter == 0)
            {
                _sqliteConnection?.Dispose();
            }
        }

        public override DbTransactionWrapper BeginTransaction()
        {
            int retry = 0;
            bool commandResult;
            SqliteTransactionWrapper result = null;
            var loopHelper = new LoopHelper { SleepTime = RetryWaitTime };
            
            do
            {
                commandResult = false;

                if (_sqliteConnection.AutoCommit)
                {
                    try
                    {
                        var wrapper = new SqliteTransactionWrapper { Transaction = _sqliteConnection?.BeginTransaction() };

                        result = wrapper;

                        commandResult = true;
                    }
                    catch (Exception e)
                    {
                        MsgLogger.Exception("BeginTransaction", e, retry);
                        commandResult = false;
                    }
                }

                if (!commandResult)
                {
                    loopHelper.Wait(true);
                    retry++;
                }
            }
            while (!commandResult && retry < 100*MaxRetryOnFailure);

            return result;
        }
        
    #endregion
    }

}

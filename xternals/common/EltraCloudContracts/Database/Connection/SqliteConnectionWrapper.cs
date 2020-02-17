/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Data;
#if SQLITE
using System.Data.SQLite;
#endif
using TanamiBot.Database.Transaction;
using TanamiBot.Helpers;

namespace TanamiBot.Database.Connection
{
#if SQLITE
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
                    //_sqliteConnection.Close();
                }
            }
        }

        public override void Dispose()
        {
            if (_referenceCounter == 0)
            {
                //_sqliteConnection?.Dispose();
            }
        }

        public override DbTransactionWrapper BeginTransaction()
        {
            int timeout = 60000;
            var loopHelper = new LoopHelper { SleepTime = 100 };

            while (!_sqliteConnection.AutoCommit)
            {
                loopHelper.Wait(true);

                timeout -= loopHelper.SleepTime;

                if (timeout < 0)
                {
                    throw new Exception("transaction timeout!");
                }
            }

            var result = new SqliteTransactionWrapper {Transaction = _sqliteConnection?.BeginTransaction()};
            
            return result;
        }
        
    #endregion
    }
#endif
}

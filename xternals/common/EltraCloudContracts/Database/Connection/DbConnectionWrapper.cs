/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Data;
using TanamiBot.Database.Transaction;

namespace TanamiBot.Database.Connection
{
    public abstract class DbConnectionWrapper : IDisposable
    {
        #region Private fields
        
        private string _connectionString;

        #endregion

        #region Constructors

        public DbConnectionWrapper(string connectionString)
        {
            MaxRetryOnFailure = 5;
            RetryWaitTime = 100;

            _connectionString = connectionString;
        }

        public DbConnectionWrapper()
        {
            MaxRetryOnFailure = 5;
            RetryWaitTime = 100;
        }

        #endregion

        #region Properties

        public virtual string ConnectionString
        {
            get => _connectionString;
            set => _connectionString = value;
        }

        public abstract ConnectionState State { get; }

        public abstract string Engine { get; }

        public int MaxRetryOnFailure { get; set; }
        public int RetryWaitTime { get; set; }

        #endregion

        #region Methods

        public virtual void Open()
        {
        }

        public virtual void Close()
        {
        }
        public virtual void Dispose()
        {
        }

        public virtual DbTransactionWrapper BeginTransaction()
        {
            return null;
        }
        
        #endregion
    }
}

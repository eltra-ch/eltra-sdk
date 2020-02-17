/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;

namespace TanamiBot.Database.Transaction
{
    public class DbTransactionWrapper : IDisposable
    {
        #region Constructors

        public DbTransactionWrapper()
        {
            MaxRetryOnFailure = 5;
            RetryWaitTime = 100;
        }

        #endregion

        #region Properties

        public int MaxRetryOnFailure { get; set; }
        public int RetryWaitTime { get; set; }
        
        #endregion

        #region Methods

        public virtual void Commit()
        {
        }
        public virtual void Dispose()
        {
        }

        #endregion
    }
}

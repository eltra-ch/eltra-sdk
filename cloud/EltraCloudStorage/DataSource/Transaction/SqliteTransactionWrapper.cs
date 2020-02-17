/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Data.SQLite;
using EltraCommon.Logger;
using EltraCloudStorage.Helpers;

namespace EltraCloudStorage.DataSource.Transaction
{
    class SqliteTransactionWrapper : DbTransactionWrapper
    {
        public SQLiteTransaction Transaction { get; set; }

        public override void Commit()
        {
            int retry = 0;
            bool commandResult;
            var loopHelper = new LoopHelper { SleepTime = RetryWaitTime };

            do
            {
                try
                {
                    Transaction.Commit();

                    commandResult = true;
                }
                catch (Exception e)
                {
                    MsgLogger.Exception("SqliteTransactionWrapper - Commit", e, retry);
                    commandResult = false;
                }

                if (!commandResult)
                {
                    loopHelper.Wait(true);
                    retry++;
                }
            }
            while (!commandResult && retry < MaxRetryOnFailure);
        }

        public override void Rollback()
        {
            Transaction?.Rollback();
        }
    }
}

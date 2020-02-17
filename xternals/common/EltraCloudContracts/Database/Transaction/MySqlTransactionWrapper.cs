/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Threading;

using MySql.Data.MySqlClient;
using TanamiBot.Helpers;

namespace TanamiBot.Database.Transaction
{
    class MySqlTransactionWrapper : DbTransactionWrapper
    {
        #region Properties

        public MySqlTransaction Transaction { get; set; }
        
        #endregion

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
                catch (MySqlException e)
                {
                    Logger.MessageLogger.Exception(GetType().Name, e, retry);
                    commandResult = false;
                }
                catch (Exception e)
                {
                    Logger.MessageLogger.Exception(GetType().Name, e, retry);
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

        public override void Dispose()
        {
            Transaction?.Dispose();
        }

    }
}

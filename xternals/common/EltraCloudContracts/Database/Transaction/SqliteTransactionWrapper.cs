/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */
#if SQLITE
using System.Data.SQLite;
 #endif

namespace TanamiBot.Database.Transaction
{
#if SQLITE
    class SqliteTransactionWrapper : DbTransactionWrapper
    {
        public SQLiteTransaction Transaction { get; set; }

        public override void Commit()
        {
            Transaction?.Commit();
        }
    }
#endif
}

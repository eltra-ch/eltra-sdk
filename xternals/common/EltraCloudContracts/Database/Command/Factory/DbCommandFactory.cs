/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using TanamiBot.Database.Connection;

namespace TanamiBot.Database.Command.Factory
{
    class DbCommandFactory
    {
        public static DbCommandWrapper GetCommand(string ident, string commandText, DbConnectionWrapper connection)
        {
            DbCommandWrapper result = null;

            if (!string.IsNullOrEmpty(commandText))
            {
                if (String.Compare(ident, "sqlite", StringComparison.OrdinalIgnoreCase) == 0)
                {
                   // result = new SqliteCommandWrapper(commandText, connection);
                }
                else if (String.Compare(ident, "mysql", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = new MySqlCommandWrapper(commandText, connection) { FixMySqlUtf8 = true };
                }
            }
            else
            {
                throw new Exception("CommandText empty!");
            }

            return result;
        }
    }
}

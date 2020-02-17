/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;

namespace TanamiBot.Database.Connection
{
    class DbConnectionFactory
    {
        #region Private fields

        private static DbConnectionWrapper _sqliteConnection;

        #endregion

        #region Methods

        public static DbConnectionWrapper GetDbConnection(string engine)
        {
            DbConnectionWrapper result = null;

            if (String.Compare(engine, "sqlite", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (_sqliteConnection == null)
                {
                    //result = new SqliteConnectionWrapper();
                    _sqliteConnection = result;
                }
                else
                {
                    result = _sqliteConnection;
                }
            }
            else if (String.Compare(engine, "mysql", StringComparison.OrdinalIgnoreCase) == 0)
            {
                result = new MySqlConnectionWrapper();
            }

            return result;
        }

        public static DbConnectionWrapper GetDbConnection(string engine, string connectionString)
        {
            DbConnectionWrapper result = null;

            if (String.Compare(engine, "sqlite", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (_sqliteConnection == null)
                {
                    //result = new SqliteConnectionWrapper(connectionString);
                    _sqliteConnection = result;
                }
                else
                {
                    result = _sqliteConnection;
                }
            }
            else if (String.Compare(engine, "mysql", StringComparison.OrdinalIgnoreCase) == 0)
            {
                result = new MySqlConnectionWrapper(connectionString);
            }

            return result;
        }

        #endregion
    }
}

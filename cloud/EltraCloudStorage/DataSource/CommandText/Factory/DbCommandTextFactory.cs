/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;

using EltraCloudStorage.DataSource.CommandText.Database;

namespace EltraCloudStorage.DataSource.CommandText.Factory
{
    class DbCommandTextFactory
    {
        public static DbCommandTextWrapper GetCommandTextWrapper(string engine, DbCommandText commandText)
        {
            DbCommandTextWrapper result = null;

            if (String.Compare(engine, "sqlite", StringComparison.OrdinalIgnoreCase) == 0)
            {
                result = new SqliteCommandTextWrapper(commandText);
            }
            else if (String.Compare(engine, "mysql", StringComparison.OrdinalIgnoreCase) == 0)
            {
                result = new MySqlCommandTextWrapper(commandText);
            }

            return result;
        }

        public static string GetCommandText(string engine, DbCommandText commandText)
        {
            var wrapper = GetCommandTextWrapper(engine, commandText);

            var result = wrapper.CommandText;

            return result;
        }

        public static string GetCommandText(string engine, DbCommandText commandText, int rowCount)
        {
            var wrapper = GetCommandTextWrapper(engine, commandText);

            wrapper.UseLimit = true;
            wrapper.LimitRowCount = rowCount;

            var result = wrapper.CommandText;

            return result;
        }

        public static string GetCommandText(string engine, DbCommandText commandText, int offset, int rowCount)
        {
            var wrapper = GetCommandTextWrapper(engine, commandText);

            wrapper.UseLimitRange = true;
            wrapper.LimitRowCount = rowCount;
            wrapper.LimitOffset = offset;

            var result = wrapper.CommandText;

            return result;
        }
    }
}

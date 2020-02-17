/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using EltraCloudStorage.DataSource.CommandText.Database.Definitions;

namespace EltraCloudStorage.DataSource.CommandText.Database
{
    class DbCommandTextDelete : DbCommandText
    {
        public DbCommandTextDelete(DeleteQuery query)
        {
            Query = query;
        }

        public DeleteQuery Query { get; set; }
    }
}

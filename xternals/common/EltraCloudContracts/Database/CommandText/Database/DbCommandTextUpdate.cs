﻿/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using TanamiBot.Database.CommandText.Database.Definitions;

namespace TanamiBot.Database.CommandText.Database
{
    class DbCommandTextUpdate : DbCommandText
    {
        public DbCommandTextUpdate(UpdateQuery query)
        {
            Query = query;
        }

        public UpdateQuery Query { get; set; }
    }
}

﻿/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;

namespace TanamiBot.Database.Reader
{
    public abstract class DbReaderWrapper : IDisposable
    {
        public abstract bool Read();

        public abstract bool IsDBNull(int index);

        public abstract int GetInt32(int index);

        public abstract object GetValue(int index);

        public abstract DateTime GetDateTime(int index);

        public abstract string GetString(int index);

        public virtual void Dispose()
        {
        }
    }
}

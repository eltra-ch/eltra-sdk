/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;

using MySql.Data.MySqlClient;

namespace EltraCloudStorage.DataSource.Reader
{
    class MySqlReaderWrapper : DbReaderWrapper
    {
        public MySqlDataReader Wrapper { get; set; }
        public override bool Read()
        {
            bool result = false;

            if (Wrapper != null)
            {
                result = Wrapper.Read();
            }

            return result;
        }

        public override bool IsDBNull(int index)
        {
            bool result = false;

            if (Wrapper != null)
            {
                result = Wrapper.IsDBNull(index);
            }

            return result;
        }

        public override int GetInt32(int index)
        {
            int result = 0;

            if (Wrapper != null)
            {
                result = Wrapper.GetInt32(index);
            }

            return result;
        }

        public override uint GetUInt32(int index)
        {
            uint result = 0;

            if (Wrapper != null)
            {
                result = Wrapper.GetUInt32(index);
            }

            return result;
        }

        public override long GetInt64(int index)
        {
            long result = 0;

            if (Wrapper != null)
            {
                result = Wrapper.GetInt64(index);
            }

            return result;
        }

        public override ulong GetUInt64(int index)
        {
            ulong result = 0;

            if (Wrapper != null)
            {
                result = Wrapper.GetUInt64(index);
            }

            return result;
        }

        public override double GetDouble(int index)
        {
            double result = 0;

            if (Wrapper != null)
            {
                result = Wrapper.GetDouble(index);
            }

            return result;
        }

        public override object GetValue(int index)
        {
            object result = null;

            if (Wrapper != null)
            {
                result = Wrapper.GetValue(index);
            }

            return result;
        }

        public override DateTime GetDateTime(int index)
        {
            DateTime result = new DateTime();

            if (Wrapper != null)
            {
                result = Wrapper.GetDateTime(index);
            }

            return result;
        }

        public override string GetString(int index)
        {
            string result = string.Empty;

            if (Wrapper != null)
            {
                result = Wrapper.GetString(index);
            }

            return result;
        }

        public override void Dispose()
        {
            Wrapper?.Dispose();
        }
    }
}

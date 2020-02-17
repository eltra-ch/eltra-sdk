/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

namespace TanamiBot.Database.Parameter
{
    class DbParameterWrapper
    {
        #region Constructors

        public DbParameterWrapper(string parameterName, object value)
        {
            ParameterName = parameterName;
            Value = value;
        }

        #endregion

        #region Properties

        public string ParameterName { get; set; }
        public object Value { get; set; }

        #endregion
    }
}

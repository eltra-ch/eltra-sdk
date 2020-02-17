/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using TanamiBot.Database.CommandText.Database;
using TanamiBot.Database.CommandText.Database.Definitions;

namespace TanamiBot.Database.CommandText
{
    abstract class DbCommandTextWrapper
    {
        #region Constructors

        public DbCommandTextWrapper(DbCommandText selection)
        {
            DbCommandText = selection;
        }
        
        #endregion

        #region Properties

        public string CommandText
        {
            get
            {
                return GetCommandText();
            }
        }

        protected SelectQuery Query
        {
            get
            {
                SelectQuery result = SelectQuery.SelectUndefined;

                if (DbCommandText is DbCommandTextSelect dbCommandText)
                {
                    result = dbCommandText.Query;
                }

                return result;
            }
        }

        protected DeleteQuery CommandTextDelete
        {
            get
            {
                DeleteQuery result = DeleteQuery.DeleteUndefined;

                if (DbCommandText is DbCommandTextDelete dbCommandText)
                {
                    result = dbCommandText.Query;
                }

                return result;
            }
        }

        protected InsertQuery CommandTextInsertion
        {
            get
            {
                InsertQuery result = InsertQuery.InsertUndefined;

                if (DbCommandText is DbCommandTextInsert dbCommandText)
                {
                    result = dbCommandText.Query;
                }

                return result;
            }
        }

        protected UpdateQuery CommandTextUpdating
        {
            get
            {
                UpdateQuery result = UpdateQuery.UpdateUndefined;

                if (DbCommandText is DbCommandTextUpdate dbCommandText)
                {
                    result = dbCommandText.Query;
                }

                return result;
            }
        }

        public bool UseLimit { get; set; }

        public bool UseLimitRange { get; set; }

        public int LimitRowCount { get; set; }

        public int LimitOffset { get; set; }
        
        public string LimitText
        {
            get
            {
                string result = string.Empty;

                if (UseLimit)
                {
                    result = $"LIMIT {LimitRowCount}";
                }
                else if (UseLimitRange)
                {
                    result = $"LIMIT {LimitOffset}, {LimitRowCount}";
                }

                return result;
            }
        }

        public DbCommandText DbCommandText { get; }
        
        #endregion

        #region Methods

        protected abstract string GetSelectCommandText();
        protected abstract string GetInsertCommandText();
        protected abstract string GetUpdateCommandText();
        protected abstract string GetDeleteCommandText();

        protected virtual string GetCommandText()
        {
            string result = string.Empty;

            if (DbCommandText is DbCommandTextSelect)
            {
                result = GetSelectCommandText();
            }

            if (DbCommandText is DbCommandTextUpdate)
            {
                result = GetUpdateCommandText();
            }

            if (DbCommandText is DbCommandTextInsert)
            {
                result = GetInsertCommandText();
            }

            if (DbCommandText is DbCommandTextDelete)
            {
                result = GetDeleteCommandText();
            }

            return result;
        }
    
        #endregion
    }
}

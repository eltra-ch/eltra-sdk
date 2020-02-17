/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Collections.Generic;
using TanamiBot.Database.Connection;
using TanamiBot.Database.Parameter;
using TanamiBot.Database.Reader;
using TanamiBot.Helpers;

namespace TanamiBot.Database.Command
{
    abstract class DbCommandWrapper : IDisposable
    {
        #region Private fields

        private List<DbParameterWrapper> _dbParameterList;

        #endregion

        #region Constructors

        public DbCommandWrapper(string commandText, DbConnectionWrapper connection)
        {
            Connection = connection;
            CommandText = commandText;
            ConnectionTimeout = 60000;
        }

        #endregion

        #region Properties

        public DbConnectionWrapper Connection { get; }
        public string CommandText { get; }

        public List<DbParameterWrapper> Parameters
        {
            get => _dbParameterList ?? (_dbParameterList = new List<DbParameterWrapper>());

            set => _dbParameterList = value;
        }

        public int ConnectionTimeout { get; set; }

        #endregion

        #region Methods

        public abstract int ExecuteNonQuery();

        public abstract object ExecuteScalar();

        public abstract DbReaderWrapper ExecuteReader();

        #region Binding

        protected abstract void BindParameters();

        #endregion

        #region Helpers

        protected void CheckConnection()
        {
            if (Connection.State == System.Data.ConnectionState.Closed ||
                Connection.State == System.Data.ConnectionState.Broken)
            {
                ReOpenConnection();
            }

            WaitForOpen();
        }

        private void WaitForOpen()
        {
            int waitTime = 0;
            var loopHelper = new LoopHelper() { SleepTime = 100 };
            while (Connection.State == System.Data.ConnectionState.Connecting || Connection.State == System.Data.ConnectionState.Executing
                                                                              || Connection.State == System.Data.ConnectionState.Fetching)
            {
                loopHelper.Wait(true);

                waitTime += loopHelper.SleepTime;

                if (waitTime > ConnectionTimeout)
                {
                    Logger.MessageLogger.WriteLine($"Connection timout {ConnectionTimeout / 1000} s occured!");
                    break;
                }
            }
        }

        private void ReOpenConnection()
        {
            Logger.MessageLogger.WriteDebug(GetType().Name, "Reconnect...");

            Connection.Open();
        }

        #endregion

        #region Dispose

        public virtual void Dispose()
        {
        }

        #endregion

        #endregion
    }
}

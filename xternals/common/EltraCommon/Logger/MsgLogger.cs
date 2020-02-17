/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using EltraCommon.Logger.Output;
using System;
using System.Diagnostics;

namespace EltraCommon.Logger
{
    public static class MsgLogger
    {
        #region Properties

        public static EltraLogger Engine { get; } = new EltraLogger();

        public static string LogPath 
        { 
            get
            {
                var fileOutput = Engine.GetOutput("File") as FileLogOutput;

                return fileOutput.LogPath;
            }
            set
            {
                var fileOutput = Engine.GetOutput("File") as FileLogOutput;

                fileOutput.LogPath = value;
            }
        }

        public static string LogFilePrefix
        {
            get
            {
                var fileOutput = Engine.GetOutput("File") as FileLogOutput;

                return fileOutput.LogFilePrefix;
            }
            set
            {
                var fileOutput = Engine.GetOutput("File") as FileLogOutput;

                fileOutput.LogFilePrefix = value;
            }
        }
                       
        public static string LogLevels
        {
            get
            {
                return Engine.Types;
            }
            set
            {
                Engine.Types = value;
            }
        }

        public static string SupportedLogLevels
        {
            get
            {
                return Engine.TypeRange;
            }
        }

        public static string LogOutputs
        {
            get
            {
                return Engine.Outputs;
            }
            set
            {
                Engine.Outputs = value;
            }
        }

        public static string SupportedLogOutputs
        {
            get
            {
                return Engine.OutputRange;
            }
        }

        #endregion

        #region Public methods

        public static void Exception(string source, Exception e)
        {
            Engine.Exception(source, e);
        }

        public static void Exception(string source, Exception e, int retryCount)
        {
            Engine.Exception(source, e, retryCount);
        }

        public static void WriteDebug(string source, string msg)
        {
            Engine.Debug(source, msg);
        }
        
        public static Stopwatch BeginTimeMeasure()
        {
            return Engine.BeginTimeMeasure();
        }

        public static void EndTimeMeasure(string source, Stopwatch stopWatch, string msg)
        {
            Engine.EndTimeMeasure(source, stopWatch, msg);
        }

        public static void WriteLine(LogMsgType type, string msg)
        {
            Engine.WriteLine(string.Empty, type, msg);
        }

        public static void WriteFlow(string msg)
        {
            Engine.WriteFlow(string.Empty, msg);
        }

        public static void Write(string source, string msg)
        {
            var nl = Engine.NewLine;

            Engine.NewLine = false;

            Engine.Info(source, msg);

            Engine.NewLine = nl;
        }

        public static void Print(string msg)
        {
            Engine.Info(string.Empty, msg);
        }

        public static void WriteLine(string msg)
        {
            Engine.WriteLine(string.Empty, msg);
        }

        public static void WriteError(string source, string msg)
        {
            Engine.Error(source, msg);
        }

        public static void WriteWarning(string source, string msg)
        {
            Engine.Warning(source, msg);
        }

        #endregion
    }
}

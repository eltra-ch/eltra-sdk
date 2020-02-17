using EltraCommon.Logger.Formatter;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace EltraCommon.Logger.Output
{
    class FileLogOutput : ILogOutput
    {
        #region Private fields

        private readonly Mutex _logMutex = new Mutex();
        private readonly ILogOutput _fallback;
        private readonly string defaultFilePrefix = "log";
        private readonly ILogFormatter _formatter;

        private bool _unauthorizedAccess;        
        private string _logFilePrefix;

        #endregion

        #region Constructors

        public FileLogOutput()
        {
            _formatter = new DefaultLogFormatter();
        }

        public FileLogOutput(ILogOutput fallback)
        {
            _fallback = fallback;
            _formatter = new DefaultLogFormatter();
        }

        #endregion

        #region Properties

        public string LogPath { get; set; }
        
        public string LogFilePrefix
        {
            get => _logFilePrefix ?? (_logFilePrefix = defaultFilePrefix);
            set => _logFilePrefix = value;
        }
        
        public string Name => "File";

        public ILogFormatter Formatter { get => _formatter; }

        #endregion

        #region Methods

        public void Write(string source, LogMsgType type, string msg, bool newLine)
        {
            string formattedMsg = Formatter.Format(source, type, msg);

            if(!string.IsNullOrEmpty(formattedMsg))
            {
                if (!_unauthorizedAccess)
                {
                    var currentProcess = Process.GetCurrentProcess();

                    if (string.IsNullOrEmpty(LogPath))
                    {
                        LogPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }

                    string errorLogPath = Path.Combine(LogPath, $"{LogFilePrefix}_{currentProcess.Id}.log");

                    _logMutex.WaitOne();

                    try
                    {
                        if (!formattedMsg.EndsWith(Environment.NewLine))
                        {
                            formattedMsg += Environment.NewLine;
                        }

                        File.AppendAllText(errorLogPath, formattedMsg);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        _unauthorizedAccess = true;

                        _fallback?.Write(source, LogMsgType.Exception, e.Message);
                    }
                    catch (Exception e)
                    {
                        _fallback?.Write(source, LogMsgType.Exception, e.Message);
                    }

                    _logMutex.ReleaseMutex();
                }
            }
        }

        #endregion
    }
}

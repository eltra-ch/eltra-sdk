using System;
using System.Threading;

namespace EltraCommon.Logger.Formatter
{
    public class DefaultLogFormatter : ILogFormatter
    {
        #region Methods

        public string Format(string source, LogMsgType type, string msg)
        {
            string time = DateTime.Now.ToString("HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            string result = string.Empty;
            int threadId = Thread.CurrentThread.ManagedThreadId;

            switch (type)
            {
                case LogMsgType.Exception:
                    {
                        if (!string.IsNullOrEmpty(source))
                        {
                            result = string.Format("{0} [{2}] [{3}] Exception: {1}", time, msg, threadId, source);
                        }
                        else
                        {
                            result = string.Format("{0} [{2}] Exception: {1}", time, msg, threadId);
                        }
                    }
                    break;
                case LogMsgType.Debug:
                    {
                        if (!string.IsNullOrEmpty(source))
                        {
                            result = string.Format("{0} [{2}] [{3}] Debug: {1}", time, msg, threadId, source);
                        }
                        else
                        {
                            result = string.Format("{0} [{2}] Debug: {1}", time, msg, threadId);
                        }
                    }
                    break;
                case LogMsgType.Error:
                    {
                        if (!string.IsNullOrEmpty(source))
                        {
                            result = string.Format("{0} [{2}] [{3}] ERROR: {1}", time, msg, threadId, source);
                        }
                        else
                        {
                            result = string.Format("{0} [{2}] ERROR: {1}", time, msg, threadId);
                        }
                    }
                    break;
                case LogMsgType.Timing:
                    {
                        if (!string.IsNullOrEmpty(source))
                        {
                            result = string.Format("{0} [{2}] [{3}] TIMING: {1}", time, msg, threadId, source);
                        }
                        else
                        {
                            result = string.Format("{0} [{2}] TIMING: {1}", time, msg, threadId);
                        }
                    }
                    break;
                case LogMsgType.Info:
                    {
                        if (!string.IsNullOrEmpty(source))
                        {
                            result = string.Format("{0} [{2}] [{3}] {1}", time, msg, threadId, source);
                        }
                        else
                        {
                            result = string.Format("{0} [{2}] {1}", time, msg, threadId);
                        }                            
                    }
                    break;
                case LogMsgType.Workflow:
                    {
                        if (!string.IsNullOrEmpty(source))
                        {
                            result = string.Format("{0} [{2}] [{3}] FLOW: {1}", time, msg, threadId, source);
                        }
                        else
                        {
                            result = string.Format("{0} [{2}] FLOW: {1}", time, msg, threadId);
                        }
                    }
                    break;
            }

            return result;
        }

        #endregion
    }
}

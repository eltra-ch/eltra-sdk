using EltraCommon.Logger;

namespace EltraCommon.Helpers
{
    internal class LogTypeHelper
    {
        public static string TypeToString(LogMsgType type)
        {
            string result = string.Empty;

            switch (type)
            {
                case LogMsgType.Debug:
                    result = "Debug";
                    break;
                case LogMsgType.Error:
                    result = "Error";
                    break;
                case LogMsgType.Exception:
                    result = "Exception";
                    break;
                case LogMsgType.Info:
                    result = "Info";
                    break;
                case LogMsgType.Warning:
                    result = "Warning";
                    break;
                case LogMsgType.Timing:
                    result = "Timing";
                    break;
                case LogMsgType.Workflow:
                    result = "Workflow";
                    break;
            }

            return result;
        }
    }
}

namespace EltraCommon.Logger
{
    public interface ILogFormatter
    {
        string Format(string source, LogMsgType type, string msg);         
    }
}

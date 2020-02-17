namespace EltraCommon.Logger
{
    public interface ILogOutput
    {
        ILogFormatter Formatter { get; }

        string Name { get; }

        void Write(string source, LogMsgType type, string msg, bool newLine = true);
    }
}

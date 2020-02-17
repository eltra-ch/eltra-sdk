using EltraCommon.Logger.Formatter;
using System;

namespace EltraCommon.Logger.Output
{
    class ConsoleLogOutput : ILogOutput
    {
        #region Private fields

        private readonly ILogFormatter _formatter;
        private bool _newLineActive;

        #endregion

        #region Constructors

        public ConsoleLogOutput()
        {
            _formatter = new DefaultLogFormatter();
        }

        #endregion

        #region Properties

        public string Name => "Console";
        public ILogFormatter Formatter { get => _formatter; }

        #endregion

        #region Methods

        public void Write(string source, LogMsgType type, string msg, bool newLine)
        {
            string formattedMsg = Formatter.Format(source, type, msg);

            if (!string.IsNullOrEmpty(formattedMsg))
            {
                if (newLine)
                {
                    if (!_newLineActive)
                    {
                        Console.Write("\r\n");
                    }

                    Console.WriteLine(formattedMsg);

                    _newLineActive = true;
                }
                else
                {
                    formattedMsg += "\r";

                    Console.Write(formattedMsg);

                    _newLineActive = false;
                }
            }            
        }

        #endregion
    }
}

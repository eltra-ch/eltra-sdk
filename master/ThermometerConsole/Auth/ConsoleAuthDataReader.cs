using EltraCloudContracts.Contracts.Users;
using System;
using System.Linq;

namespace RelayMaster.Auth
{
    class ConsoleAuthDataReader
    {
        public bool ReadLogin(out string login)
        {
            bool result;

            char[] allowedCharacters = { '@', '.' };

            result = ReadFromConsole(allowedCharacters, out login);

            return result;
        }

        public bool ReadPassword(out string pass)
        {
            bool result;

            char[] allowedCharacters = { '@', '.', ',', '!', '#', '-', '+' };

            result = ReadFromConsole(allowedCharacters, out pass);

            return result;
        }

        private static bool ReadFromConsole(char[] allowedCharacters, out string text)
        {
            bool result = false;
            ConsoleKeyInfo cki;

            text = string.Empty;

            do
            {
                cki = Console.ReadKey(true);

                if (cki.Key == ConsoleKey.Backspace && text.Length > 0)
                {
                    var startPos = Console.CursorLeft;
                    text = text.Substring(0, text.Length - 1);
                    Console.SetCursorPosition(startPos - 1 - text.Length, Console.CursorTop);
                    Console.Write($"{text} ");
                    Console.CursorLeft = Console.CursorLeft - 1;
                }
                else if (char.IsLetterOrDigit(cki.KeyChar) || allowedCharacters.Contains(cki.KeyChar))
                {
                    text += cki.KeyChar;
                    Console.Write(cki.KeyChar);
                }

            } while (cki.Key != ConsoleKey.Escape && cki.Key != ConsoleKey.Enter);

            if (cki.Key != ConsoleKey.Escape && !string.IsNullOrEmpty(text))
            {
                result = true;
            }

            return result;
        }
    }
}

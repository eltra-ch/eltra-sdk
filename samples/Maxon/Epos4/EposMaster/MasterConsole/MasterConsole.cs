using EltraCommon.Contracts.Users;
using System;

namespace EposMaster.MasterConsole
{
    public class MasterConsole
    {
        public static bool CheckAuthData(ref UserIdentity authData)
        {
            bool result = false;

            if (string.IsNullOrEmpty(authData.Login) ||
               authData.Login == "?" ||
               string.IsNullOrEmpty(authData.Password))
            {
                var consoleAuthDataReader = new AuthDataReader();

                Console.Write("Login:");

                if (consoleAuthDataReader.ReadLogin(out var login))
                {
                    Console.WriteLine("");
                    Console.Write("Password:");

                    if (consoleAuthDataReader.ReadPassword(out var pass))
                    {
                        Console.WriteLine("");

                        authData.Login = login;
                        authData.Password = pass;

                        result = true;
                    }
                }
            }
            else
            {
                result = true;
            }

            return result;
        }
    }
}

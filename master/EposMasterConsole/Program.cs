using System;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Users;
using EposMaster.Settings;
using EposMaster.Auth;
using EposMaster;

namespace EposMasterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new MasterSettings();

            var host = settings.Host;
            var authData = settings.Auth.AuthData;
            var appName = AppDomain.CurrentDomain.FriendlyName;
            var master = new EposMaster.EposMaster(settings);

            MsgLogger.Print($"{appName}");

            ParseCommandLineParams(args, ref host, ref authData);

            if (CheckAuthData(ref authData))
            {
                MsgLogger.Print($"host='{host}', user={authData.Login}, pwd='{authData.PlainPassword}'");

                _ = master.Start(host, authData);

                MsgLogger.Print("press any key to close...");

                Console.ReadKey();

                master.Stop();
            }
            else
            {
                MsgLogger.Print($"you have to enter login and password to proceed!");
            }

            MsgLogger.Print("Exit");
        }

        private static bool CheckAuthData(ref UserAuthData authData)
        {
            bool result = false;

            if(string.IsNullOrEmpty(authData.Login) ||
               authData.Login == "?" ||
               string.IsNullOrEmpty(authData.PlainPassword))
            {
                var consoleAuthDataReader = new ConsoleAuthDataReader();

                Console.Write("Login:");

                if(consoleAuthDataReader.ReadLogin(out var login))
                {
                    Console.WriteLine("");
                    Console.Write("Password:");

                    if(consoleAuthDataReader.ReadPassword(out var pass))
                    {
                        Console.WriteLine("");

                        authData.Login = login;
                        authData.PlainPassword = pass;
                         
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

        private static void ParseCommandLineParams(string[] args, ref string host, ref UserAuthData authData)
        {
            if (args.Length > 0)
            {
                host = args[0];

                if (args.Length > 2)
                {
                    authData.Login = args[1];
                    authData.PlainPassword = args[2];
                }
            }
        }
    }
}

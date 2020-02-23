using System;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Users;
using ThermoMaster.Settings;
using RelayMaster.Auth;
using ThermoMaster;
using CommandLine;
using System.Collections.Generic;
using EltraCommon.Ipc;

namespace ThermoMasterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptionsAndReturnExitCode)
                .WithNotParsed(HandleParseError);

            MsgLogger.WriteFlow( "Exit");
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            var settings = new MasterSettings();

            var host = settings.Host;
            var authData = settings.Auth.AuthData;
            var appName = AppDomain.CurrentDomain.FriendlyName;
            var master = new ThermoMaster.ThermoMaster(settings);

            if (!string.IsNullOrEmpty(opts.Login))
            {
                authData.Login = opts.Login;
            }

            if (!string.IsNullOrEmpty(opts.Password))
            {
                authData.PlainPassword = opts.Password;
            }

            if (!string.IsNullOrEmpty(opts.Host))
            {
                host = opts.Host;
            }

            if(opts.Stop)
            {
                var client = new NpClient() { Name = appName, Timeout = 3000 };

                if (client.Stop())
                {
                    MsgLogger.WriteFlow("stop request sent successfully!");
                }
                else
                {
                    MsgLogger.WriteError("RunOptionsAndReturnExitCode", "stop request sending failed!"); 
                }
            }
            else
            {
                MsgLogger.WriteFlow($"Start '{appName}'");

                var npServer = new NpServer() { Name = appName };

                if (npServer.Start())
                {
                    npServer.StepRequested += (sender, e) => 
					{ 
						MsgLogger.WriteFlow("stop request received ...");
						
						master.Stop(); 
					};

                    if (CheckAuthData(ref authData))
                    {
                        MsgLogger.WriteFlow($"host='{host}', user={authData.Login}, pwd='{authData.PlainPassword}', timeout = {settings.Timeout}");

                        master.Start(host, authData).Wait();
                    }
                    else
                    {
                        MsgLogger.WriteFlow("you have to enter login and password to proceed!");
                    }

                    npServer.Stop();
                }
                else
                {
                    Console.WriteLine("start failed!");
                }
            }            
        }
        
        private static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine("Command line parsing errors:");

            foreach (var err in errs)
            {
                Console.WriteLine(err);
            }
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
    }
}

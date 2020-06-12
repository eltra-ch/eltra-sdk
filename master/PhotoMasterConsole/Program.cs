using System;
using EltraCommon.Logger;
using PhotoMaster.Settings;
using CommandLine;
using System.Collections.Generic;
using EltraMaster.MasterConsole;
using EltraMaster;
using PhotoMaster.DeviceManager;

namespace PhotoMasterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<MasterOptions>(args)
                .WithParsed(RunOptionsAndReturnExitCode)
                .WithNotParsed(HandleParseError);

            MsgLogger.WriteFlow( "Exit");
        }
        
        private static void RunOptionsAndReturnExitCode(MasterOptions opts)
        {
            var appName = AppDomain.CurrentDomain.FriendlyName;
            var settings = new MasterSettings();
            var host = settings.Host;
            string login = settings.Auth.AuthData.Login;
            string password = settings.Auth.AuthData.Password;

            if (!string.IsNullOrEmpty(opts.Host))
            {
                host = opts.Host;
            }

            if (!string.IsNullOrEmpty(opts.Login))
            {
                login = opts.Login;
            }

            if (!string.IsNullOrEmpty(opts.Password))
            {
                password = opts.Password;
            }

            var master = new EltraMasterConnector();

            if (!master.RunAsService(appName, new PhotoDeviceManager(settings), host, settings.UpdateInterval, settings.Timeout, login, settings.Auth.Name, password))
            {
                MsgLogger.WriteError(appName, "starting master service failed!");
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
    }
}

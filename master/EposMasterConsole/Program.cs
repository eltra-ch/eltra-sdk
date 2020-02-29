using System;
using EltraCommon.Logger;
using EposMaster.Settings;
using CommandLine;
using System.Collections.Generic;
using EltraMaster.MasterConsole;
using EltraMaster;
using EposMaster.DeviceManager;

namespace EposMasterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<MasterOptions>(args)
                .WithParsed(RunOptionsAndReturnExitCode)
                .WithNotParsed(HandleParseError);

            MsgLogger.WriteFlow("Exit");
        }

        private static void RunOptionsAndReturnExitCode(MasterOptions opts)
        {
            var appName = AppDomain.CurrentDomain.FriendlyName;
            var settings = new MasterSettings();
            var host = settings.Host;
            string login = settings.Auth.AuthData.Login;
            string password = settings.Auth.AuthData.PlainPassword;

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

            if (!master.RunAsService(appName, new EposDeviceManager(settings), host, settings.UpdateInterval, settings.Timeout, login, settings.Auth.Name, password))
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

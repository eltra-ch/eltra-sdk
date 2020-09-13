using System;
using EltraCommon.Logger;
using EposMaster.Settings;
using CommandLine;
using System.Collections.Generic;
using EposMaster.DeviceManager;
using EltraConnector.Master;
using EltraConnector.Master.Device.Connection;
using EltraCommon.Contracts.Users;
using EposMaster.MasterConsole;
using System.Threading.Tasks;

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
            string login = settings.Identity.AuthData.Login;
            string password = settings.Identity.AuthData.Password;

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

            master.Host = host;
            master.ConnectionSettings = new ConnectionSettings() { UpdateInterval = settings.UpdateInterval, Timeout = settings.Timeout };

            var t = Task.Run(async () => {
                
                Console.WriteLine($"Sign-in login={login}, passwd={password}");

                var di = new UserIdentity() { Login = login, Name = settings.Identity.Name, Password = password, Role = settings.Identity.Role };

                if (await master.SignIn(di, true))
                {
                    if (!master.StartService(appName, new EposDeviceManager(settings)))
                    {
                        MsgLogger.WriteError(appName, "starting master service failed!");
                    }
                }
            });

            t.Wait();
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

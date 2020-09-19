using System;
using System.Threading.Tasks;
using EltraCommon.Contracts.Users;
using EltraConnector.Events;
using EltraConnector.Master;
using EltraConnector.Master.Definitions;
using EltraConnector.Master.Events;

namespace StreemaMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            string serviceName = "STREEMA";

            var connector = new EltraMasterConnector();
            var settings = new StreemaSettings();

            Console.WriteLine($"Hello Dummy Eltra Master - {serviceName}!");

            var runner = Task.Run(async () =>
            {
                connector.Host = settings.Host;

                Console.WriteLine("Sign-in ...");

                if (await connector.SignIn(new UserIdentity() { Login = settings.Login, Name = "Streema", Password = settings.LoginPasswd, Role="developer" }, true))
                {
                    var predefinedAlias = new UserIdentity() { Login = settings.Alias, Password = settings.AliasPasswd, Name = "Streema client", Role = "engineer" };

                    if (await connector.CreateAlias(predefinedAlias))
                    {
                        Console.WriteLine($"User-defined alias, login='{predefinedAlias.Login}', password='{predefinedAlias.Password}'");
                    }
                    else
                    {
                        Console.WriteLine("ERROR: user defined alias not created!");
                    }

                    connector.StatusChanged += OnConnectorStatusChanged;
                    connector.ChannelStatusChanged += OnChannelStatusChanged;
                    connector.RemoteChannelStatusChanged += OnRemotePartyChannelStatusChanged;

                    Console.WriteLine("Signed in - Start service");

                    connector.StartService(serviceName, new StreemaDeviceManager(settings.XddFile, settings));
                }
                else
                {
                    Console.WriteLine("error: cannot sign-in!");
                }
            });

            Console.ReadKey();

            connector.StopService(serviceName);

            runner.Wait();
        }

        private static void OnRemotePartyChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            Console.WriteLine($"remote channel {e.Id} status changed {e.Status}");
        }

        private static void OnChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            Console.WriteLine($"my channel {e.Id} status changed {e.Status}");
        }

        private static void OnConnectorStatusChanged(object sender, MasterStatusEventArgs e)
        {
            if(e.Status == MasterStatus.Started)
            {
                Console.WriteLine("started, press any key to finish!");
            }
            else
            {
                Console.WriteLine($"connector status changed {e.Status}");
            }
        }
    }
}

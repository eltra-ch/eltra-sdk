using System;
using System.Threading.Tasks;
using EltraCommon.Contracts.Users;
using EltraConnector.Events;
using EltraConnector.Master;
using EltraConnector.Master.Definitions;
using EltraConnector.Master.Events;

namespace TestMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            var connector = new EltraMasterConnector();
            string filePath = "TEST_0200h_0000h_0000h_0000h.xdd";

            Console.WriteLine($"Hello Test Eltra Unit Tests Master!");

            var runner = Task.Run(async () =>
            {
                connector.Host = "https://eltra.ch";

                if (args.Length > 0)
                {
                    connector.Host = args[0];
                }
                
                Console.WriteLine("Sign-in ...");

                if (await connector.SignIn(new UserIdentity() { Login = $"test.master2@eltra.ch", Name = "Test", Password = "1234", Role="developer" }, true))
                {
                    var predefinedAlias = new UserIdentity() { Login = "test2@eltra.ch", Password = "1234", Name = "Tester", Role = "engineer" };

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

                    connector.StartService("TEST", new TestDeviceManager(filePath));
                }
                else
                {
                    Console.WriteLine("error: cannot sign-in!");
                }
            });

            Console.ReadKey();

            connector.StopService("TEST");

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

using System;
using System.Threading.Tasks;
using EltraCommon.Contracts.Users;
using EltraConnector.Events;
using EltraConnector.Master;
using EltraConnector.Master.Definitions;
using EltraConnector.Master.Events;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var connector = new EltraMasterConnector();
            int serviceId = 1;
            string serviceName = "DUMMY#1";

            if(args.Length>0)
            {
                if(int.TryParse(args[0], out int si))
                {
                    serviceId = si;
                }
            }

            serviceName = $"DUMMY#{serviceId}";

            string filePath = "DUMMY_0100h_0000h_0000h_0000h.xdd";

            Console.WriteLine($"Hello Dummy Eltra Master - {serviceName}!");

            var runner = Task.Run(async () =>
            {
                connector.Host = "https://eltra.ch";
                //connector.Host = "http://localhost:5001";

                Console.WriteLine("Sign-in ...");

                if (await connector.SignIn(new UserIdentity() { Login = $"tst{serviceId}master@eltra.ch", Name = "Dummy", Password = "1234", Role="developer" }, true))
                {
                    var predefinedAlias = new UserIdentity() { Login = "abcd1@eltra.ch", Password = "1234", Name = "dummy", Role = "engineer" };

                    if (await connector.CreateAlias(predefinedAlias))
                    {
                        Console.WriteLine($"User-defined alias, login='{predefinedAlias.Login}', password='{predefinedAlias.Password}'");
                    }
                    else
                    {
                        Console.WriteLine("ERROR: user defined alias not created!");
                    }

                    var alias = await connector.CreateAlias("operator");

                    if(alias!=null)
                    {
                        Console.WriteLine($"Alias, login='{alias.Login}', password='{alias.Password}'");
                    }
                    else
                    {
                        Console.WriteLine("ERROR: alias not created!");
                    }
                    
                    connector.StatusChanged += OnConnectorStatusChanged;
                    connector.ChannelStatusChanged += OnChannelStatusChanged;
                    connector.RemoteChannelStatusChanged += OnRemotePartyChannelStatusChanged;

                    Console.WriteLine("Signed in - Start service");

                    connector.StartService(serviceName, new DummyDeviceManager(filePath));
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

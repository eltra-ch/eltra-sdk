using System;
using System.Threading.Tasks;
using EltraCommon.Contracts.Users;
using EltraConnector.Master;
using EltraConnector.Master.Events;
using EltraConnector.Master.Status;

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

            string[] urls = new string[] { "https://eltra.ch", "http://localhost:5001" };

            Console.WriteLine($"Hello Dummy Eltra Master - {serviceName}!");

            var runner = Task.Run(() =>
            {
                connector.Host = urls[0];
                connector.AuthData = new UserData() { Login= $"dummy{serviceId}@eltra.ch", Name="Dummy", Password = "1234" };
                
                connector.StartService(serviceName, new DummyDeviceManager(filePath));
            });

            connector.StatusChanged += Connector_StatusChanged; 

            Console.ReadKey();

            connector.StopService(serviceName);

            runner.Wait();
        }

        private static void Connector_StatusChanged(object sender, MasterStatusEventArgs e)
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

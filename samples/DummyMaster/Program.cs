using System;
using System.Threading.Tasks;
using EltraMaster.Events;
using EltraMaster.Status;
using EltraMaster;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Users;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var connector = new EltraMasterConnector();
            const string ServiceName = "DUMMY#1";

            const uint updateInterval = 60;
            const uint timeout = 180;
            string filePath = "DUMMY_0100h_0000h_0000h_0000h.xdd";

            string[] urls = new string[] { "https://eltra.ch", "http://localhost:5001" };

            Console.WriteLine($"Hello Dummy Eltra Master - {ServiceName}!");

            var runner = Task.Run(() =>
            {
                connector.Host = urls[0];
                connector.AuthData = new UserAuthData() { Login= "dummy@eltra.ch", Name="Dummy", Password = "1234" };

                connector.StartService(ServiceName, new DummyDeviceManager(filePath), updateInterval, timeout);
            });

            connector.StatusChanged += Connector_StatusChanged; 

            Console.ReadKey();

            connector.StopService(ServiceName);

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

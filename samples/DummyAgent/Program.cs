using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.Contracts.Users;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using System;
using System.Threading.Tasks;
using EltraCommon.Logger;

namespace DummyAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DeviceVcs vcs = null;
            string paramUniqueId = string.Empty;

            var t = Task.Run(async ()=>
            {
                var myAuth = new UserAuthData() { Login = "dummy@eltra.ch", Password = "1234" };
                var deviceAuth = new UserAuthData() { Login = "dummy@eltra.ch", Password = "1234" };

                var agent = new DeviceAgent("https://eltra.ch", myAuth, 60, 120);

                var sessionDevices = await agent.GetDevices(deviceAuth);

                foreach(var sd in sessionDevices)
                {
                    Session session = sd.Item1;
                    EltraDevice device = sd.Item2;

                    Console.WriteLine($"session = {session.Uuid}, device = {device.Name}");

                    vcs = new DeviceVcs(agent, device);

                    var parameter = vcs.SearchParameter(0x3000, 0x00) as Parameter;

                    parameter.ParameterChanged += Parameter_ParameterChanged;

                    paramUniqueId = parameter.UniqueId;

                    Console.WriteLine($"register parameter = {parameter.UniqueId}");

                    vcs.RegisterParameterUpdate(parameter.UniqueId, ParameterUpdatePriority.High);

                    Console.WriteLine($"get command - start counting");

                    var command = await vcs.Agent.GetDeviceCommand(device, "StartCounting");

                    command.SetParameterValue<int>("Step", 10);
                    command.SetParameterValue<int>("Delay", 100);

                    Console.WriteLine($"execute command - start counting");

                    await agent.ExecuteCommand(command);

                    Console.WriteLine($"wait ...");

                    await Task.Delay(30000);
                }

            });

            t.Wait();

            vcs?.UnregisterParameterUpdate(paramUniqueId);

        }

        private static void Parameter_ParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            var parameterValue = e.NewValue;
            int val = 0;

            Console.WriteLine($"parameter changed {e.Parameter.UniqueId}");

            try
            {
                if (parameterValue.GetValue(ref val))
                {
                    Console.WriteLine($"{val}");
                }
            }
            catch(Exception)
            {
            }
        }
    }
}

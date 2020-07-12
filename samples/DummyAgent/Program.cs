using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Contracts.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using System;
using System.Threading.Tasks;
using EltraConnector.Agent;
using EltraConnector.Extensions;

namespace DummyAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string[] urls = new string[] { "https://eltra.ch", "http://localhost:5001" };

            int sessionId = 1;

            if(args.Length>0)
            {
                if(int.TryParse(args[0], out int si))
                {
                    sessionId = si;
                }
            }

            var agentAuth = new UserAuthData() { Login = $"agent{sessionId}@eltra.ch", Password = "1234" };

            AgentConnector connector = new AgentConnector() { Host = urls[1], AuthData = agentAuth };
            string paramUniqueId = string.Empty;
            int nodeId = 1;

            var t = Task.Run(async ()=>
            {
                var deviceAuth = new UserAuthData() { Login = $"dummy{sessionId}@eltra.ch", Password = "1234" };
                
                var deviceNodeList = await connector.GetDevices(deviceAuth, nodeId);

                foreach(var deviceNode in deviceNodeList)
                {
                    var device = deviceNode;

                    Console.WriteLine($"device = {device.Name}");

                    var parameter = device.SearchParameter(connector, 0x3000, 0x00) as Parameter;

                    if (parameter == null)
                        break;

                    parameter.ParameterChanged += OnParameterChanged;

                    paramUniqueId = parameter.UniqueId;

                    Console.WriteLine($"register parameter = {parameter.UniqueId}");

                    connector.RegisterParameterUpdate(device, parameter.UniqueId, ParameterUpdatePriority.High);

                    Console.WriteLine($"get all supported commands");

                    var commands = await connector.GetDeviceCommands(deviceNode);

                    if (commands != null)
                    {
                        foreach (var cmd in commands)
                        {
                            Console.WriteLine($"command = {cmd.Name}");
                        }
                    }

                    Console.WriteLine($"get command - start counting");

                    var command = await connector.GetDeviceCommand(deviceNode, "StartCounting");

                    if (command != null)
                    {
                        command.SetParameterValue("Step", 333);
                        command.SetParameterValue("Delay", 1000);

                        Console.WriteLine($"execute command - start counting");

                        await connector.ExecuteCommand(command);

                        Console.WriteLine($"wait ...");

                        await Task.Delay(30000);
                    }

                    command = await connector.GetDeviceCommand(deviceNode, "StopCounting");

                    if (command != null)
                    {
                        await connector.ExecuteCommand(command);

                        connector?.UnregisterParameterUpdate(device, paramUniqueId);
                    }
                }

            });

            t.Wait();
        }

        private static void OnParameterChanged(object sender, ParameterChangedEventArgs args)
        {
            var parameterValue = args.NewValue;
            int val = 0;

            Console.WriteLine($"parameter changed {args.Parameter.UniqueId}");

            try
            {
                if (parameterValue.GetValue(ref val))
                {
                    Console.WriteLine($"{val}");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

using EltraCommon.Contracts.Devices;
using System;
using System.Threading.Tasks;

namespace DummyAgent
{
    class SampleCommands
    {
        private readonly EltraDevice _device;

        public SampleCommands(EltraDevice device)
        {
            _device = device;
        }

        private async Task ListAllCommands()
        {
            Console.WriteLine($"get all supported commands");

            var commands = await _device.GetCommands();

            if (commands != null)
            {
                foreach (var cmd in commands)
                {
                    Console.WriteLine($"command = {cmd.Name}");
                }
            }
        }

        private async Task CallStartCountingCommand(int step = 3, int delay = 250)
        {
            var command = await _device.GetCommand("StartCounting");

            if (command != null)
            {
                command.SetParameterValue<int>("Step", step); //increase the value by step
                command.SetParameterValue<int>("Delay", delay); //delay between each step

                Console.WriteLine($"execute command - start counting");

                await command.Execute();
            }
        }

        private async Task CallStopCountingCommand()
        {
            var command = await _device.GetCommand("StopCounting");

            if (command != null)
            {
                await command.Execute();
            }
        }

        public async Task PlayWithDeviceCommands()
        {
            await ListAllCommands();

            Console.WriteLine($"get command - start counting");

            await CallStartCountingCommand();

            Console.WriteLine($"wait ...");

            await Task.Delay(5000);

            await CallStopCountingCommand();
        }
    }
}

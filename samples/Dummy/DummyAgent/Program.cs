using System;
using System.Threading.Tasks;

namespace DummyAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Dummy Agent!");

            // Get initial parameters, sessionId has default value 1
            // (in case you would like to start multiple processes on the same workstation)
            GetStartParameters(args, out int sessionId);

            string host = "https://eltra.ch";
            //string host = "http://localhost:5001";

            //run demo async mode
            var demoTask = StartDemoAsync(host, sessionId);

            demoTask.Wait();
        }

		static Task StartDemoAsync(string url, int sessionId)
        {
            var task = Task.Run(async () =>
            {
                //sample dummy implementation
                var agent = new SampleAgent();

                //connect to eltra service, sign-in etc.
                if (await agent.Connect(url, sessionId))
                {
                    //grab and play with sample device
                    await agent.PlayWithDevices();

                    //sign-out, close session
                    await agent.Disconnect();
                }
                else
                {
                    Console.WriteLine("ERROR: Connect failed!");
                }
            });

            return task;
        }
		
        static void GetStartParameters(string[] args, out int sessionId)
        {
            sessionId = 1;

            if (args.Length > 0)
            {
                if (int.TryParse(args[1], out int sessionIdx))
                {
                    sessionId = sessionIdx;
                }
            }
        }
    }
}

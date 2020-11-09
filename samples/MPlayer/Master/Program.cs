﻿using System;
using System.Threading.Tasks;
using EltraCommon.Contracts.Users;
using EltraConnector.Events;
using EltraConnector.Master;
using EltraConnector.Master.Definitions;
using EltraConnector.Master.Events;
using EltraConnector.Master.Device.Connection;
using EltraCommon.Logger;
using MPlayerMaster.Device;

namespace MPlayerMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            string serviceName = "MPLAYER";

            var connector = new EltraMasterConnector();
            var settings = new MPlayerSettings();

            MsgLogger.LogLevels = settings.LogLevels;
            MsgLogger.LogOutputs = settings.LogOutputs;

            Console.WriteLine($"Hello MPlayer Eltra Master - {serviceName}!");

            var runner = Task.Run(async () =>
            {
                connector.Host = settings.Host;

                Console.WriteLine("Sign-in ...");

                if (await connector.SignIn(new UserIdentity() { Login = settings.Login, Name = "Mplayer", Password = settings.LoginPasswd, Role="developer" }, true))
                {
                    var predefinedAlias = new UserIdentity() { Login = settings.Alias, Password = settings.AliasPasswd, Name = "MPlayer client", Role = "engineer" };

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

                    //heartbeat every 3 min, timeout device after 9 min
                    connector.ConnectionSettings = new ConnectionSettings() { UpdateInterval = 180, Timeout = 540 };

                    connector.StartService(serviceName, new MPlayerDeviceManager(settings.XddFile, settings));
                }
                else
                {
                    Console.WriteLine("error: cannot sign-in!");
                }
            });

            /*if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.ReadKey();

                connector.StopService(serviceName);
            }*/

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


using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Channels.Events;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Users;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraConnector.Agent;
using System;
using System.Threading.Tasks;

namespace DummyAgent
{
    class SampleAgent
    {
        #region Private members

        private AgentConnector _connector;

        #endregion

        #region Events

        private void OnParameterChanged(object sender, ParameterChangedEventArgs args)
        {
            var parameterValue = args.NewValue;
            int val = 0;

            try
            {
                if (parameterValue.GetValue(ref val))
                {
                    Console.WriteLine($"parameter value changed {args.Parameter.UniqueId}, 0x{args.Parameter.Index:X4}:0x{args.Parameter.SubIndex:X2}, new value = {val}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void OnChannelStatusChanged(object sender, ChannelStatusChangedEventArgs e)
        {
            var channel = sender as Channel;

            Console.WriteLine($"channel {channel.Id}, created by {channel.UserName} status {e.Status}");
            Console.WriteLine($"channel city {channel.Location.City}, country {channel.Location.Country}");
        }

        #endregion

        #region Methods

        public async Task<bool> Connect(string host, int sessionId)
        {
            bool result = false;
            var signInData = new UserIdentity() { Login = $"agent{sessionId}@eltra.ch", Password = "1234" };

            _connector = new AgentConnector() { Host = host };

            if (await _connector.SignIn(signInData, true))
            {
                var deviceIdentity = new UserIdentity() { Login = "abcd1@eltra.ch", Password = "1234" };

                if (!await _connector.Connect(deviceIdentity))
                {
                    Console.WriteLine("uups, no connection, timeout!");
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }
        
        private async Task PlayWithParameterStatistics(EltraDevice device)
        {
            //another approach to find parameter instance in object dictionary - by parameter unique id
            var counterParameter = device.SearchParameter("PARAM_Counter") as Parameter;

            //get some statistics of the parameter lifetime
            var stats = await counterParameter.GetValueHistoryStatistics(DateTime.MinValue, DateTime.Now);

            if (stats != null)
            {
                Console.WriteLine($"items count = {stats.EntriesCount}, size in bytes {stats.SizeInBytes}, {stats.Created}");
            }
        }

        public async Task<bool> PlayWithDevice(EltraDevice device)
        {
            bool result = false;

            Console.WriteLine($"device = {device.Name}, node id = {device.NodeId}, version = {device.Version}, serial number = {device.Identification.SerialNumber:X8}");

            //0x3000, 0x00 is address of the counter parameter in object dictionary
            var counterParameter = device.SearchParameter(0x3000, 0x00) as Parameter;
            var controlWordParameter = device.SearchParameter(0x6040, 0x00) as Parameter;
            var cp = await device.GetParameter(0x3000, 0x00) as Parameter;

            if (counterParameter != null && controlWordParameter != null)
            {
                const ushort readyFlag = 1;
                const ushort sleepingFlag = 0;

                //let's get the current value stored in local object dictionary (can be outdated)
                controlWordParameter.GetValue(out ushort controlWord);

                // force parameter update, ok, now we are sure that the value is synchronized with master object dictionary
                await controlWordParameter.ReadValue();

                //control word is defined as UINT16 - RW, let's modify value of this parameter in local object dictionary
                controlWordParameter.SetValue(readyFlag);

                //synchronize value with master object dictionary
                await controlWordParameter.Write();

                // this method is reliable, but to keep the local object dictionary up-to-date not quiete effective
                // (object dictionary can contain more than 100 parameters)   
                // your agent usually, doesn't need the current value information about all parameters
                // to limit the read requests, you can specify explicite the parameters you are interested in
                Console.WriteLine($"register parameter 0x{counterParameter.Index:X4}:0x{counterParameter.SubIndex:X2} for updates");

                //we would like to be informed each time the parameter is changed
                counterParameter.ParameterChanged += OnParameterChanged;
                
                // to activate this feature, call RegisterUpdate
                // priority is transfered to the master and is up to master to decide what low, high or medium priority is
                counterParameter.AutoUpdate(true, ParameterUpdatePriority.High);
                //from now on, our local object dictionary will be actualized each time the {counterParameter} is changed by remote party

                // execute command on the remote master device, 
                // our dummy master has 2 simple methods, start and stop counting
                // let's start counting and observe how our counter parameter {counterParameter} is changing ...
                // on the end, we will stop counting, to give our master some peace ...
                var sampleCommands = new SampleCommands(device);

                await sampleCommands.PlayWithDeviceCommands();

                //we don't need the notifications at this point
                counterParameter?.AutoUpdate(false, ParameterUpdatePriority.High);

                //remove events handling
                counterParameter.ParameterChanged -= OnParameterChanged;

                //in case our parameter is supporting <backup> flag, we can grab some historic data
                await PlayWithParameterStatistics(device);

                //reset control word parameter value
                controlWordParameter.SetValue(sleepingFlag);

                result = true;
            }

            return result;
        }

        public async Task PlayWithDevices()
        {
            var channels = await _connector.GetChannels();

            Console.WriteLine($"channels count = {channels.Count}");

            foreach (var channel in channels)
            {
                //let's grab some informations about our channel
                Console.WriteLine($"channel found = {channel.Id}, owner = {channel.UserName}");
                Console.WriteLine($"channel location = {channel.Location.City}, {channel.Location.Country}, {channel.Location.Latitude}, {channel.Location.Longitude}");

                //we would like to receive the notification if the channel is going offline
                channel.StatusChanged += OnChannelStatusChanged;

                Console.WriteLine($"devices count = {channel.Devices.Count}");

                //list the devices attached to channel
                foreach (var device in channel.Devices)
                {
                    await PlayWithDevice(device);
                }
            }
        }

        public async Task Disconnect()
        {
            await _connector.SignOut();
        }

        #endregion
    }
}

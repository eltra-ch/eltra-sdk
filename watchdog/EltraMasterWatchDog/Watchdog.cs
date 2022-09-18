using System;
using System.Threading.Tasks;
using EltraCommon.Contracts.Users;
using EltraCommon.Logger;
using EltraConnector.Agent;
using EltraCommon.Contracts.Channels;

namespace EltraMasterWatchDog
{
    class Watchdog
    {
        private WatchdogSettings _settings;
        private AgentConnector _connector;

        public Watchdog(WatchdogSettings settings)
        {
            _settings = settings;
        }

        private async Task<ChannelsState> GetChannelsState()
        {
            const string methodId = "GetChannelsState";

            var channels = await _connector.GetChannels();
            var result = new ChannelsState();

            foreach (var channel in channels)
            {
                MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"found channel: {channel.Id}, last access = {channel.Modified} status = {channel.Status}");

                if (channel.Modified > result.LastDeviceOnline)
                {
                    result.LastDeviceOnline = channel.Modified;
                }

                if (channel.Status == ChannelStatus.Online)
                {
                    result.IsDeviceOnline = true;
                    break;
                }
            }

            return result;
        }

        private async Task<bool> Connect()
        {
            const string methodId = "Connect";

            bool result = false;
            var agentAuth = new UserIdentity() { Login = _settings.AgentLogin, Password = _settings.AgentPassword, Name = "watchdog", Role = "developer" };
            
            _connector = new AgentConnector() { Host = _settings.Host };

            MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Sign-in user: '{agentAuth.Name}' ...");

            if (await _connector.SignIn(agentAuth, true))
            {
                var deviceAuth = new UserIdentity() { Login = _settings.DeviceLogin, Password = _settings.DevicePassword };

                MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Connect to device: '{deviceAuth.Login}' ...");

                if (await _connector.Connect(deviceAuth))
                {
                    MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Successfully connected to device: '{deviceAuth.Login}'");

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {methodId}", $"Connection to device: {deviceAuth.Login} failed!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {methodId}", $"User {agentAuth.Login} cannot sign-in!");
            }

            return result;
        }

        private async Task Disconnect()
        {
            const string methodId = "Disconnect";

            MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Disconnecting...");

            _connector.Disconnect();

            MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Signing out...");

            await _connector.SignOut();
        }

        private async Task Process()
        {
            const string methodId = "Process";

            MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Connect");

            if (await Connect())
            {
                MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Validate channels");

                await ValidateChannels();
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {methodId}", $"Connect failed!");
            }

            MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Disconnect");

            await Disconnect();
        }

        private async Task ValidateChannels()
        {
            const string methodId = "ValidateChannels";

            var channelsState = await GetChannelsState();

            var inactivityTimeInMinutes = (int)Math.Round((DateTime.Now - channelsState.LastDeviceOnline).TotalMinutes);

            MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"device status, online = {channelsState.IsDeviceOnline}, last modified = {channelsState.LastDeviceOnline}, inactivity time {inactivityTimeInMinutes} [min]");

            bool isDeviceInactive = !channelsState.IsDeviceOnline && inactivityTimeInMinutes > _settings.MaxInactivityTimeInMinutes;

            if (isDeviceInactive)
            {
                if(!Respawn())
                {
                    MsgLogger.WriteError($"{GetType().Name} - {methodId}", "respawn failed!");
                }
            }
        }

        private bool Respawn()
        {
            const string methodId = "Respawn";
            bool result = false;

            MsgLogger.WriteWarning($"{GetType().Name} - {methodId}", $"device unactive for {_settings.MaxInactivityTimeInMinutes} [min], try kill stalled proccess");

            var masterProcess = new MasterProcess();

            if (masterProcess.Kill(_settings))
            {
                result = masterProcess.Respawn(_settings);
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {methodId}", $"killing the process {_settings.MasterProcess} failed!");
            }

            return result;
        }

        public void Run()
        {
            const string methodId = "Run";

            var watchdogTask = Task.Run(async () => {

                await Process();

            });

            MsgLogger.WriteLine($"{GetType().Name} - {methodId}", "starting");

            watchdogTask.Wait();
        }
    }
}

using EltraCommon.Logger;

namespace EltraMasterWatchDog
{
    class Program
    {
        const string appName = "EltraMasterWatchdog";

        private static WatchdogSettings _settings;

        private static WatchdogSettings Settings
        {
            get => _settings ?? (_settings = new WatchdogSettings());
        }

        static void Main(string[] args)
        {
            MsgLogger.WriteLine(appName, "start");

            var watchdog = new Watchdog(Settings);

            watchdog.Run();

            MsgLogger.WriteLine(appName, "exit");
        }
    }
}
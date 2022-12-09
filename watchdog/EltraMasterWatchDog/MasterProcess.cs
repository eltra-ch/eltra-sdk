using EltraCommon.Logger;
using System;
using System.Diagnostics;
using System.IO;

namespace EltraMasterWatchDog
{
    class MasterProcess
    {
        public bool Respawn(WatchdogSettings settings)
        {
            const string methodId = "Respawn";
            bool result = false;

            var fi = new FileInfo(settings.MasterProcess);

            if (fi.Exists)
            {
                var process = Process.Start(new ProcessStartInfo()
                {
                    WorkingDirectory = fi.DirectoryName,
                    CreateNoWindow = false,
                    UseShellExecute = true,
                    FileName = fi.FullName
                });

                if (process != null)
                {
                    MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Master process {settings.MasterProcess} successfully started!");
                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {methodId}", $"Master process {settings.MasterProcess} cannot be started!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {methodId}", $"Master process {settings.MasterProcess} doesn't exist!");
            }

            return result;
        }

        public bool Kill(WatchdogSettings settings)
        {
            const string methodId = "Kill";
            bool result = false;

            try
            {
                var fi = new FileInfo(settings.MasterProcess);

                if (fi.Exists)
                {
                    var processes = Process.GetProcessesByName(fi.Name);

                    if (processes.Length > 0)
                    {
                        MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"found '{fi.Name}' processes {processes.Length} count");
                    }
                    else
                    {
                        MsgLogger.WriteWarning($"{GetType().Name} - {methodId}", $"Process '{fi.Name}' not found!");
                    }

                    foreach (var process in processes)
                    {
                        MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"try exit '{fi.Name}' with process id = {process.Id}");

                        process.Kill(true);
                    }

                    result = true;
                }
                else
                {
                    MsgLogger.WriteLine($"{GetType().Name} - {methodId}", $"Master process {settings.MasterProcess} doesn't exist!");

                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - {methodId}", e);
            }

            return result;
        }
    }
}

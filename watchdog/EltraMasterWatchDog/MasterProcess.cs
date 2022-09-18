using EltraCommon.Logger;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace EltraMasterWatchDog
{
    class MasterProcess
    {

        

        public void Respawn(WatchdogSettings settings)
        {
            const string methodId = "Respawn";

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
        }

        public void Kill(WatchdogSettings settings)
        {
            const string methodId = "Kill";

            try
            {
                FileInfo fi = new FileInfo(settings.MasterProcess);

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
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {methodId}", $"Master process {settings.MasterProcess} doesn't exist!");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - {methodId}", e);
            }
        }
    }
}

using EltraCommon.Contracts.CommandSets;
using System;

namespace EltraConnector.Controllers.Commands
{
    class ExecuteCommandCacheEntry
    {
        public ExecuteCommandCacheEntry()
        {
        }

        public ExecuteCommandCacheEntry(DeviceCommand deviceCommand)
        {
            if (deviceCommand != null)
            {
                Status = deviceCommand.Status;
                Timestamp = DateTime.Now;
            }
        }

        public ExecCommandStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

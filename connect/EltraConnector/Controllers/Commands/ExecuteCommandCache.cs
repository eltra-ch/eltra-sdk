using EltraCommon.Contracts.CommandSets;
using EltraCommon.Logger;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EltraConnector.Controllers.Commands
{
    class ExecuteCommandCache
    {
        private Dictionary<string, List<ExecuteCommandCacheEntry>> _cache;
        private readonly SemaphoreSlim _lock;

        public ExecuteCommandCache()
        {
            _cache = new Dictionary<string, List<ExecuteCommandCacheEntry>>();
            _lock = new SemaphoreSlim(1);
        }

        public bool CanExecute(ExecuteCommand command)
        {
            bool result = false;
            
            _lock.Wait();

            if (command != null && !string.IsNullOrEmpty(command.CommandId))
            {
                var deviceCommand = command.Command;

                if (!_cache.ContainsKey(command.CommandId))
                {
                    _cache.Add(command.CommandId, new List<ExecuteCommandCacheEntry> { new ExecuteCommandCacheEntry(deviceCommand) } );

                    result = true;
                }
                else
                {
                    var entryList = _cache[command.CommandId];

                    if(entryList != null && deviceCommand != null)
                    {
                        result = true;

                        foreach (var entry in entryList)
                        {
                            if (entry.Status == deviceCommand.Status)
                            {
                                result = false;
                                break;
                            }
                        }
                    }
                }

                Cleanup();
            }

            _lock.Release();

            if(!result)
            {
                MsgLogger.WriteLine($"{GetType().Name} - CanExecute", $"drop command");
            }

            return result;
        }

        private void RemoveCacheEntry(ExecuteCommandCacheEntry entry)
        {
            foreach(var ci in _cache)
            {
                var entryList = ci.Value;

                foreach (var e in entryList)
                {
                    if (e == entry)
                    {
                        entryList.Remove(e);
                        break;
                    }
                }

                if(entryList.Count == 0)
                {
                    _cache.Remove(ci.Key);
                    break;
                }
            }
        }

        private void Cleanup()
        {
            const double MaxCacheLifetimeInSec = 60;
            
            var toRemove = new List<ExecuteCommandCacheEntry>();
            
            foreach (var item in _cache)
            {
                var entryList = item.Value;

                foreach (var entry in entryList)
                {
                    if ((DateTime.Now - entry.Timestamp).TotalSeconds > MaxCacheLifetimeInSec)
                    {
                        toRemove.Add(entry);
                    }
                }
            }

            foreach (var entry in toRemove)
            {
                RemoveCacheEntry(entry);
            }
        }
    }
}

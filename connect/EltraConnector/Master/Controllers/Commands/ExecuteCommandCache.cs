using EltraCommon.Contracts.CommandSets;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EltraConnector.Master.Controllers.Commands
{
    class ExecuteCommandCache
    {
        private Dictionary<string, DateTime> _cache;
        private readonly SemaphoreSlim _lock;

        public ExecuteCommandCache()
        {
            _cache = new Dictionary<string, DateTime>();
            _lock = new SemaphoreSlim(1);
        }

        public bool CanExecute(ExecuteCommand command)
        {
            bool result = false;
            
            _lock.Wait();

            if (command != null && !string.IsNullOrEmpty(command.CommandId))
            {
                bool containsEntry = _cache.ContainsKey(command.CommandId);

                if (!containsEntry)
                {
                    _cache.Add(command.CommandId, DateTime.Now);
                }

                Cleanup();

                result = !containsEntry;
            }

            _lock.Release();

            return result;
        }

        private void Cleanup()
        {
            const double MaxCacheLifetimeInSec = 60;
            
            var toRemove = new List<string>();
            
            foreach (var c in _cache)
            {
                if ((DateTime.Now - c.Value).TotalSeconds > MaxCacheLifetimeInSec)
                {
                    toRemove.Add(c.Key);
                }
            }

            foreach (var c in toRemove)
            {
                _cache.Remove(c);
            }
        }
    }
}

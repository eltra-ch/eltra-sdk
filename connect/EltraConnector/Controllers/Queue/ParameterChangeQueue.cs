using System.Collections.Generic;

namespace EltraConnector.Controllers.Queue
{
    class ParameterChangeQueue
    {
        private readonly List<ParameterChangeQueueItem> _parameterChangeQueue;
        private readonly object _parameterChangeQueueLock;

        public ParameterChangeQueue()
        {
            _parameterChangeQueue = new List<ParameterChangeQueueItem>();
            _parameterChangeQueueLock = new object();
        }

        public void Add(ParameterChangeQueueItem queueItem)
        {
            lock (_parameterChangeQueueLock)
            {
                _parameterChangeQueue.Add(queueItem);
            }
        }

        private void Cleanup()
        {
            lock (_parameterChangeQueueLock)
            {
                var toRemove = new List<ParameterChangeQueueItem>();

                foreach (var queueItem in _parameterChangeQueue)
                {
                    if(queueItem.WorkingTask.IsCompleted)
                    {
                        toRemove.Add(queueItem);
                    }
                }

                foreach(var qi in toRemove)
                {
                    _parameterChangeQueue.Remove(qi);
                }
            }
        }

        internal bool ShouldSkip(ParameterChangeQueueItem queueItem)
        {
            bool skipProcessing = false;

            Cleanup();

            lock (_parameterChangeQueueLock)
            {
                foreach (var qi in _parameterChangeQueue)
                {
                    if (qi.Equals(queueItem) && queueItem != qi)
                    {
                        if (queueItem.Timestamp < qi.Timestamp)
                        {
                            skipProcessing = true;
                            break;
                        }
                    }
                }
            }

            return skipProcessing;
        }
    }
}

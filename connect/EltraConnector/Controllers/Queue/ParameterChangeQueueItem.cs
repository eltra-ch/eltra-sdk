using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using System;
using System.Threading.Tasks;

namespace EltraConnector.Controllers.Queue
{
    class ParameterChangeQueueItem
    {
        private ParameterValue _actualValue;
        private readonly long _timestamp;
        private Task _workingTask;

        public ParameterChangeQueueItem(int nodeId, Parameter parameter)
        {
            NodeId = nodeId;
            ActualValue = parameter.ActualValue;
            UniqueId = parameter.UniqueId;
            Index = parameter.Index;
            SubIndex = parameter.SubIndex;
            _timestamp = DateTime.Now.Ticks;
        }

        public int NodeId { get; set; }
        public ushort Index { get; set; }
        public byte SubIndex { get; set; }
        public string UniqueId { get; set; }
        public ParameterValue ActualValue
        {
            get => _actualValue;
            set
            {
                _actualValue = value.Clone();
            }
        }
        public long Timestamp { get => _timestamp; }

        public Task WorkingTask 
        { 
            get => _workingTask;
            set => _workingTask = value;
        }

        public bool Equals(ParameterChangeQueueItem item)
        {
            bool result = false;

            if (NodeId == item.NodeId && Index == item.Index && SubIndex == item.SubIndex)
            {
                result = true;
            }

            return result;
        }
    }
}
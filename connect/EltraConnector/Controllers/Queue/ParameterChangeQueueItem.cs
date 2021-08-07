using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using System;
using System.Threading.Tasks;

namespace EltraConnector.Controllers.Queue
{
    class ParameterChangeQueueItem
    {
        #region Private fields

        private ParameterValue _actualValue;
        private readonly long _timestamp;
        private Task _workingTask;
        private Parameter _parameter;
        private ParameterValue _newValue;
        private ParameterValue _oldValue;

        #endregion

        #region Constructors

        public ParameterChangeQueueItem(int nodeId, Parameter parameter, ParameterValue newValue, ParameterValue oldValue)
        {
            NodeId = nodeId;

            _parameter = parameter;
            _newValue = newValue.Clone();
            _oldValue = oldValue.Clone();

            UniqueId = parameter.UniqueId;
            Index = parameter.Index;
            SubIndex = parameter.SubIndex;

            _timestamp = DateTime.Now.Ticks;
        }

        #endregion

        #region Properties

        public int NodeId { get; set; }
        public ushort Index { get; set; }
        public byte SubIndex { get; set; }
        public string UniqueId { get; set; }
        public ParameterValue NewValue => _newValue;
        public long Timestamp { get => _timestamp; }
        public Task WorkingTask 
        { 
            get => _workingTask;
            set => _workingTask = value;
        }

        #endregion

        #region Methods

        public bool Equals(ParameterChangeQueueItem item)
        {
            bool result = false;

            if (NodeId == item.NodeId && Index == item.Index && SubIndex == item.SubIndex)
            {
                result = true;
            }

            return result;
        }

        public async Task<bool> Update()
        {
            bool result = false;

            if (_parameter != null)
            {
                if (await _parameter.SetParameterValue(NewValue))
                {
                    result = true;
                }
                else
                {
                    _parameter.SetValue(_oldValue);
                }
            }

            return result;
        }

        #endregion
    }
}
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Templates;
using System;

namespace EltraConnector.Classes
{
    class RegisteredParameter
    {
        #region Private fields

        private readonly object _syncObject;
        private readonly ushort _index;
        private readonly byte _subIndex;
        private readonly Flags _flags;

        private int _instanceCount;
        private DateTime _lastModified;

        #endregion

        #region Constrcutors

        public RegisteredParameter(string uniqueId, ushort index, byte subIndex, Flags flags, object syncObject)
        {
            UniqueId = uniqueId;
            _index = index;
            _subIndex = subIndex;
            _flags = flags;
            _syncObject = syncObject;
            _lastModified = DateTime.MinValue;

            InstanceCount = 0;
            MaxCacheDelayInSec = 10;
        }

        #endregion

        #region Properties

        public string UniqueId { get; private set; }

        public ushort Index => _index;

        public byte SubIndex => _subIndex;

        public int InstanceCount
        {
            get => _instanceCount;
            set
            {
                lock(_syncObject)
                {
                    if (value >= 0)
                    {
                        _instanceCount = value;
                    }
                }
            }
        }

        public DateTime LastModified
        {
            get => _lastModified;
            set
            {
                lock (_syncObject)
                {
                    _lastModified = value;
                }
            }
        }

        public bool CanUseCache
        {
            get
            {
                bool result = false;

                if(_flags.Volatile != 0 && DateTime.Now - LastModified < TimeSpan.FromSeconds(MaxCacheDelayInSec))
                {
                    result = true;
                }

                return result;
            }
        }

        public double MaxCacheDelayInSec { get; set; }

        #endregion

        #region Methods

        internal int Release()
        {
            if (InstanceCount > 0)
            {
                InstanceCount--;
            }
            else
            {
                InstanceCount = 0;
            }

            return InstanceCount;
        }

        internal void Reset()
        {
            LastModified = DateTime.MinValue;
        }

        #endregion
    }
}

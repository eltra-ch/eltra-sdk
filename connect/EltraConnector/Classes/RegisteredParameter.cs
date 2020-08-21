namespace EltraConnector.Classes
{
    class RegisteredParameter
    {
        private readonly object _syncObject;
        private int _instanceCount;

        public RegisteredParameter(string uniqueId, object syncObject)
        {
            UniqueId = uniqueId;

            _syncObject = syncObject;

            InstanceCount = 1;
        }

        public string UniqueId { get;set; }
        public int InstanceCount
        {
            get => _instanceCount;
            set
            {
                lock(_syncObject)
                {
                    _instanceCount = value;
                }
            }
        }
    }
}

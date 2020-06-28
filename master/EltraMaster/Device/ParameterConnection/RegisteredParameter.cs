using System.Diagnostics;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;

namespace EltraMaster.Device.ParameterConnection
{
    class RegisteredParameter
    {
        #region Private fields

        private Stopwatch _stopwatch;
        
        #endregion

        #region Constructors

        public RegisteredParameter()
        {
            Restart();
        }

        #endregion

        #region Properties

        public string Source { get; set; }

        public ParameterBase Parameter { get; set; }

        public ParameterUpdatePriority Priority { get; set; }

        public int RefCount { get; set; }

        public long ElapsedTime => _stopwatch.ElapsedMilliseconds;

        public string Key => $"{Source}_{Parameter.UniqueId}";
        
        #endregion

        #region Methods

        public void Restart()
        {
            _stopwatch = new Stopwatch();

            _stopwatch.Start();
        }

        #endregion
    }
}

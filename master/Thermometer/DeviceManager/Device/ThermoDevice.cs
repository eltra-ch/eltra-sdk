using EltraMaster.Device;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThermoMaster.DeviceManager.SensorConnection;
using ThermoMaster.Settings;
using Thermometer.DeviceManager.Device.Commands;

namespace Thermometer.DeviceManager.Device
{
    sealed class ThermoDevice : MasterDevice
    {
        #region Private fields

        private SensorConnectionManager _sensorConnectionManager;

        #endregion

        #region Constructors

        public ThermoDevice(MasterSettings settings)
            : base("THERMO", settings.Device.XddFile)
        {
            Settings = settings;

            Identification.SerialNumber = settings.Device.SerialNumber;

            ExtendCommandSet();
        }

        #endregion

        #region Properties

        public MasterSettings Settings { get; }

        #endregion

        #region Methods

        private void ExtendCommandSet()
        {
            AddCommand(new GetTemperatureCommand(this));
            AddCommand(new GetHumidityCommand(this));
        }

        protected override void OnInitialized()
        {
            _sensorConnectionManager = new SensorConnectionManager(this, CloudAgent) { Settings = Settings.Device };
        }

        protected override void CreateCommunication()
        {
            Communication = new ThermoDeviceCommunication(this, Settings);
        }

        public override void Disconnect()
        {
            if (_sensorConnectionManager.IsRunning)
            {
                _sensorConnectionManager.Stop();
            }

            base.Disconnect();
        }

        protected override void StartConnectionManagersAsync(ref List<Task> tasks)
        {
            var task = _sensorConnectionManager.StartAsync();

            if (task != null)
            {
                tasks.Add(task);
            }
        }

        public bool GetInternalSample(out ThermoSensorsSample sample)
        {
            bool result = false;

            sample = null;

            if (_sensorConnectionManager != null)
            {
                result = _sensorConnectionManager.GetInternalSample(out sample);
            }

            return result;
        }

        public bool GetBme280Sample(out ThermoSensorsSample sample)
        {
            bool result = false;

            sample = null;

            if (_sensorConnectionManager != null)
            {
                if (_sensorConnectionManager.GetInternalSample(out sample) && sample.Source == SampleSource.bme280)
                {
                    result = true;
                }
                else if (_sensorConnectionManager.GetExternalSample(out sample) && sample.Source == SampleSource.bme280)
                {
                    result = true;
                }
            }

            return result;
        }

        public bool GetExternalSample(out ThermoSensorsSample sample)
        {
            bool result = false;

            sample = null;

            if (_sensorConnectionManager != null)
            {
                result = _sensorConnectionManager.GetExternalSample(out sample);
            }

            return result;
        }

        #endregion
    }
}

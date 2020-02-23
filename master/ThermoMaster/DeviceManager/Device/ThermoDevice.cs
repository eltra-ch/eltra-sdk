using EltraCommon.Logger;
using System;
using ThermoMaster.DeviceManager.Wrapper;
using ThermoMaster.Settings;
using EltraMaster.Os.Linux;
using ThermoMaster.DeviceManager.SensorConnection;
using EltraMaster.Device;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThermoMaster.DeviceManager.Device.Commands;

namespace ThermoMaster.DeviceManager.Device
{
    sealed class ThermoDevice : MasterDevice
    {
        #region Private fields

        private SensorConnectionManager _sensorConnectionManager;
        private readonly MasterSettings _settings;

        #endregion

        #region Constructors

        public ThermoDevice(MasterSettings settings)
            : base("THERMO", settings.Device.XddFile)
        {
            _settings = settings;
            
            Pins = _settings.Device.Pins.ToArray();

            Identification.SerialNumber = _settings.Device.SerialNumber;

            CreateCommandSet();
        }

        #endregion

        #region Properties

        public MasterSettings Settings { get => _settings; }

        public int[] Pins
        {
            get
            {
                int[] result = null;

                if (Communication is ThermoDeviceCommunication communication)
                {
                    result = communication.RelayPins;
                }

                return result;
            }
            set
            {
                if (Communication is ThermoDeviceCommunication communication)
                {
                    communication.RelayPins = value;
                }

                OnPinsChanged();
            }
        }

        #endregion

        #region Methods

        private void OnPinsChanged()
        {
            try
            {
                if (SystemHelper.IsLinux)
                {
                    EltraRelayWrapper.Initialize();

                    foreach (var pin in Pins)
                    {
                        EltraRelayWrapper.RelayPinMode((ushort)pin, EltraRelayWrapper.GPIOpinmode.Output);
                    }
                }
                else
                {
                    MsgLogger.WriteLine(LogMsgType.Warning, "GPIO library is not supported on windows");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - OnPinsChanged", e);
            }
        }

        private void CreateCommandSet()
        {            
            //Object
            AddCommand(new GetChannelStateCommand(this));
            AddCommand(new SetChannelStateCommand(this));

            AddCommand(new GetTemperatureCommand(this));
            AddCommand(new GetHumidityCommand(this));

            AddCommand(new RegisterParameterUpdateCommand(this));
            AddCommand(new UnregisterParameterUpdateCommand(this));

            AddCommand(new GetObjectCommand(this));
            AddCommand(new SetObjectCommand(this));
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            CreateSensorConnectionManager();
        }

        private void CreateSensorConnectionManager()
        {
            _sensorConnectionManager = new SensorConnectionManager(this, CloudAgent) { Settings = Settings.Device };
        }

        protected override void CreateCommunication()
        {
            Communication = new ThermoDeviceCommunication(this, Settings);
        }

        protected override void StartConnectionManagersAsync(ref List<Task> tasks)
        {
            var task = _sensorConnectionManager.StartAsync();

            if (task != null)
            {
                tasks.Add(task);
            }
        }

        public override void Disconnect()
        {
            if (_sensorConnectionManager.IsRunning)
            {
                _sensorConnectionManager.Stop();
            }

            base.Disconnect();
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

        public bool GetBmp180Sample(out ThermoSensorsSample sample)
        {
            bool result = false;

            sample = null;

            if (_sensorConnectionManager != null)
            {
                if (_sensorConnectionManager.GetInternalSample(out sample) && sample.Source == SampleSource.Bmp180)
                {
                    result = true;
                }
                else if (_sensorConnectionManager.GetExternalSample(out sample) && sample.Source == SampleSource.Bmp180)
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

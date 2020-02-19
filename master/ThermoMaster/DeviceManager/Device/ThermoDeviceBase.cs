using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.SyncAgent;
using ThermoMaster.DeviceManager.Device.Thermostat.ObjectDictionary;
using ThermoMaster.DeviceManager.ParameterConnection;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThermoMaster.DeviceManager.SensorConnection;
using EltraCommon.Logger;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Events;
using ThermoMaster.Settings;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;

namespace ThermoMaster.DeviceManager.Device
{
    abstract class ThermoDeviceBase : EltraDevice
    {
        #region Private fields
        
        private SyncCloudAgent _cloudAgent;
        private SensorConnectionManager _sensorConnectionManager;
        private MasterSettings _settings;

        #endregion

        #region Constructors

        public ThermoDeviceBase(string name, MasterSettings settings)
        {
            _settings = settings;

            Name = name;
        }
        
        #endregion

        #region Properties
        
        public ThermoDeviceCommunication Communication { get; set; }

        public ParameterConnectionManager ParameterConnectionManager { get; private set; }

        public SyncCloudAgent CloudAgent
        {
            get => _cloudAgent;
            set
            {
                _cloudAgent = value;

                OnCloudAgentChanged();
            }
        }

        public int[] Pins 
        { 
            get
            {
                int[] result = null;

                if (Communication!=null)
                { 
                    result = Communication.RelayPins;
                }

                return result;
            }
            set
            {
                if (Communication != null)
                {
                    Communication.RelayPins = value;
                }

                OnPinsChanged();
            }
        }

        public MasterSettings Settings { get => _settings; }

        #endregion

        #region Events handling

        protected virtual void OnCloudAgentChanged()
        {
            CreateCommunication();
            CreateDeviceDescription();
        }

        protected virtual void OnPinsChanged()
        {
        }

        private void OnDeviceDescriptionStateChanged(object sender, DeviceDescriptionEventArgs e)
        {
            if(e.State == DeviceDescriptionState.Read)
            {                
                CreateObjectDictionary();
                CreateConnectionManager();
            }
        }

        #endregion

        #region Methods

        private void CreateConnectionManager()
        {
            ParameterConnectionManager = new ParameterConnectionManager(this);

            _sensorConnectionManager = new SensorConnectionManager(this, _cloudAgent) { Settings = Settings.Device };            
        }

        public override async void CreateDeviceDescription()
        {
            DeviceDescription = new XddDeviceDescriptionFile(this);

            DeviceDescription.Url = CloudAgent.Url;
            DeviceDescription.SourceFile = Settings.Device.XddFile;

            DeviceDescription.StateChanged += OnDeviceDescriptionStateChanged;

            await DeviceDescription.Read();
        }

        public override bool CreateObjectDictionary()
        {
            var objectDictionary = new ThermostatObjectDictionary(this);

            bool result = objectDictionary.Open();

            if (objectDictionary.Open())
            {
                ObjectDictionary = objectDictionary;

                Status = DeviceStatus.Ready;
                
                result = true;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - CreateObjectDictionary", "Cannot open object dictionary!");
            }

            return result;
        }

        private void CreateCommunication()
        {
            Communication = new ThermoDeviceCommunication(this, Settings);
        }

        public override async void RunAsync()
        {
            var tasks = new List<Task>();

            StartParameterConnectionManager(ref tasks);

            StartSensorConnectionManager(ref tasks);

            await Task.WhenAll(tasks);
        }

        public void Disconnect()
        {
            if (ParameterConnectionManager.IsRunning)
            {
                ParameterConnectionManager.Stop();
            }

            if(_sensorConnectionManager.IsRunning)
            {
                _sensorConnectionManager.Stop();
            }

            Status = DeviceStatus.Disconnected;
        }

        private void StartParameterConnectionManager(ref List<Task> tasks)
        {
            var task = ParameterConnectionManager.StartAsync();

            if (task != null)
            {
                tasks.Add(task);
            }
        }

        private void StartSensorConnectionManager(ref List<Task> tasks)
        {
            var task = _sensorConnectionManager.StartAsync();

            if (task != null)
            {
                tasks.Add(task);
            }
        }

        public bool ReadParameter(Parameter parameter)
        {
            bool result = false;

            if (ParameterConnectionManager != null)
            {
                result = ParameterConnectionManager.ReadParameter(parameter);
            }

            return result;
        }

        public bool WriteParameter(Parameter parameter)
        {
            bool result = false;

            if (ParameterConnectionManager != null)
            {
                result = ParameterConnectionManager.WriteParameter(parameter);
            }

            return result;
        }

        public bool ReadParameterValue<T>(Parameter parameter, out T value)
        {
            bool result = false;

            value = default;

            if (ParameterConnectionManager != null)
            {
                if (ParameterConnectionManager.ReadParameter(parameter))
                {
                    result = parameter.GetValue(out value);
                }
            }

            return result;
        }

        public bool WriteParameterValue<T>(Parameter parameter, T value)
        {
            bool result = false;

            if (ParameterConnectionManager != null)
            {
                if (parameter.SetValue(value))
                {
                    result = ParameterConnectionManager.WriteParameter(parameter);
                }
            }

            return result;
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
                if(_sensorConnectionManager.GetInternalSample(out sample) && sample.Source == SampleSource.Bmp180)
                {
                    result = true;
                }
                else if(_sensorConnectionManager.GetExternalSample(out sample) && sample.Source == SampleSource.Bmp180)
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

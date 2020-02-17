using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.Logger;
using EltraCommon.Threads;
using EltraConnector.SyncAgent;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ThermoMaster.DeviceManager.Device;
using ThermoMaster.DeviceManager.Wrapper;
using ThermoMaster.Settings;

namespace ThermoMaster.DeviceManager.SensorConnection
{
    class SensorConnectionManager : EltraThread
    {
        #region Private fields

        private readonly ThermoDeviceBase _device;
        private readonly SyncCloudAgent _cloudAgent;
        private ThermoSensorsManager _dhtManager;

        private Parameter _actualTemperatureParameter;
        private Parameter _actualExternalTemperatureParameter;
        private Parameter _temperatureTimestampParameter;
        private Parameter _temperatureMinParameter;
        private Parameter _temperatureMaxParameter;
        private Parameter _actualHumidityParameter;
        private Parameter _samplingTimeParameter;
        private Parameter _relayState1Parameter;

        private Parameter _actualPm25Parameter;
        private Parameter _actualPm10Parameter;

        private readonly InertiaManager _inertiaManager;

        private DeviceSettings _settings;

        #endregion

        #region Constructors

        public SensorConnectionManager(ThermoDeviceBase device, SyncCloudAgent cloudAgent)
        {
            _device = device;
            _cloudAgent = cloudAgent;
            _inertiaManager = new InertiaManager(device);
        }

        #endregion

        #region Events

        private void OnTemperatureChanged(object sender, ParameterChangedEventArgs e)
        {            
            MsgLogger.WriteFlow( $"Temperature changed, new temperature = {ActualTemperature}");

            if (ActualTemperature < TemperatureMin)
            {
                MsgLogger.WriteFlow( $"Temperature lower that min, temperature = {ActualTemperature}/{TemperatureMin}");

                _inertiaManager.StartMinCheck();
            }
            else if (ActualTemperature > TemperatureMax)
            {
                MsgLogger.WriteFlow( $"Temperature higher that max, temperature = {ActualTemperature}/{TemperatureMax}");

                _inertiaManager.StartMaxCheck();
            }
            else if(ActualTemperature > TemperatureMin && ActualTemperature < TemperatureMax)
            {
                MsgLogger.WriteFlow( $"Temperature in range, temperature = {ActualTemperature}/[{TemperatureMin}, {TemperatureMax}]");

                _inertiaManager.StartInRangeCheck();
            }
        }

        #endregion

        #region Properties

        double TemperatureMin
        {
            get
            {
                double result = 0;

                if(_temperatureMinParameter!=null)
                {
                    _temperatureMinParameter.GetValue(out result);
                }

                return result;
            }
        }

        double TemperatureMax
        {
            get
            {
                double result = 0;

                if (_temperatureMaxParameter != null)
                {
                    _temperatureMaxParameter.GetValue(out result);
                }

                return result;
            }
        }

        public DeviceSettings Settings 
        { 
            get => _settings;
            set
            {
                _settings = value;

                OnSettingsChanged();
            } 
        }
        
        public double ActualTemperature
        {
            get
            {
                double result = double.MinValue;

                if (_actualTemperatureParameter != null)
                {
                    if(_actualTemperatureParameter.GetValue(out double temperature))
                    {
                        result = temperature;
                    }
                }

                return result;
            }
        }

        public double ActualHumidity
        {
            get
            {
                double result = double.MinValue;

                if (_actualHumidityParameter != null)
                {
                    if (_actualHumidityParameter.GetValue(out double humidity))
                    {
                        result = humidity;
                    }
                }

                return result;
            }
        }

        #endregion

        #region Events

        protected virtual void OnSettingsChanged()
        {
            if (_dhtManager != null)
            {
                _dhtManager.Settings = Settings;
            }
        }

        #endregion

        #region Methods

        private bool GetSamplingTime(out ushort samplingTime)
        {
            bool result = false;
            
            samplingTime = 0;

            if (_samplingTimeParameter != null)
            {
                result = _samplingTimeParameter.GetValue(out samplingTime);
            }

            return result;
        }

        private bool InitParameters()
        {            
            _actualHumidityParameter = _device.SearchParameter("PARAM_ActualHumidity") as Parameter;
            _actualTemperatureParameter = _device.SearchParameter("PARAM_ActualTemperature") as Parameter;
            _actualExternalTemperatureParameter = _device.SearchParameter("PARAM_ActualExternalTemperature") as Parameter;
            _temperatureTimestampParameter = _device.SearchParameter("PARAM_TemperatureTimestamp") as Parameter;
            _samplingTimeParameter = _device?.SearchParameter("PARAM_SamplingTime") as Parameter;
            _temperatureMinParameter = _device?.SearchParameter("PARAM_TemperatureMinThreshold") as Parameter;
            _temperatureMaxParameter = _device?.SearchParameter("PARAM_TemperatureMaxThreshold") as Parameter;
            _relayState1Parameter = _device.SearchParameter("PARAM_RelayState_1") as Parameter;
            //Particulate Matter Sensor
            _actualPm25Parameter = _device.SearchParameter("PARAM_ActualPm25") as Parameter;
            _actualPm10Parameter = _device.SearchParameter("PARAM_ActualPm10") as Parameter;

            if (_actualTemperatureParameter != null)
            {
                _actualTemperatureParameter.ParameterChanged += OnTemperatureChanged;
            }

            if(_temperatureTimestampParameter != null)
            {
                _temperatureTimestampParameter.ParameterChanged += OnTemperatureChanged;
            }

            return _actualHumidityParameter != null && _actualTemperatureParameter != null && _samplingTimeParameter != null;
        }
        
        protected override Task Execute()
        {
            const int minWaitInterval = 100;
            const ushort ChannelOn = 0;
            const ushort ChannelOff = 1;

            try
            {
                InitParameters();

                StartDht22Manager();

                _inertiaManager.Start();

                if(!RelayTransition.MakeTransition(_device, _relayState1Parameter, ChannelOn, ChannelOff))
                {
                    MsgLogger.WriteError($"{GetType().Name} - Execute", $"make relay {_relayState1Parameter.UniqueId} transition from {ChannelOn} to {ChannelOff} failed!");
                }

                while (ShouldRun())
                {
                    ushort samplingTime = 0;
                    
                    if (GetSamplingTime(out samplingTime))
                    {
                        if (!UpdateSensorData())
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Execute", "Update sensor data failed!");
                        }

                        var validateIntervalWatch = new Stopwatch();

                        validateIntervalWatch.Start();

                        while (validateIntervalWatch.Elapsed < TimeSpan.FromSeconds(samplingTime) && ShouldRun())
                        {
                            if (GetSamplingTime(out var st) && st != samplingTime)
                            {
                                samplingTime = st;
                            }

                            Thread.Sleep(minWaitInterval);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Execute", e);
            }

            _inertiaManager.Stop();

            StopDht22Manager();

            SetStopped();

            return base.Execute();
        }

        private bool UpdateSensorData()
        {
            bool result = UpdateTemperature();

            result &= UpdateHumidity();

            result &= UpdateExternalTemperature();

            result &= UpdatePressure();

            result &= UpdateParticulateMatterSensor();

            return result;
        }

        private bool UpdateHumidity()
        {
            bool result = false;
                        
            if (GetInternalSample(out var sample))
            {
                if (sample.Source == SampleSource.Dht22)
                {
                    if (_actualHumidityParameter != null)
                    {
                        result = _actualHumidityParameter.SetValue(sample.Humidity);
                    }

                    var parameterEntry = _device.SearchParameter("PARAM_ActualHumidityAveraged") as Parameter;

                    if (result && parameterEntry != null)
                    {
                        result = parameterEntry.SetValue(sample.HumidityAveraged);
                    }

                    parameterEntry = _device.SearchParameter("PARAM_HumidityTimestamp") as Parameter;

                    if (result && parameterEntry != null)
                    {
                        result = parameterEntry.SetValue(sample.Timestamp);
                    }
                }
                else
                {
                    result = true;
                }
            }
            
            return result;
        }

        private bool UpdateTemperature()
        {
            bool result = false;
                       
            if (GetInternalSample(out var sample))
            {
                if (sample.Source == SampleSource.Dht22 || sample.Source == SampleSource.Bmp180)
                {
                    if (_actualTemperatureParameter != null)
                    {
                        result = _actualTemperatureParameter.SetValue(sample.Temperature);
                    }

                    var parameterEntry = _device.SearchParameter("PARAM_ActualTemperatureAveraged") as Parameter;

                    if (result && parameterEntry != null)
                    {
                        result = parameterEntry.SetValue(sample.TemperatureAveraged);
                    }

                    parameterEntry = _device.SearchParameter("PARAM_TemperatureTimestamp") as Parameter;

                    if (result && parameterEntry != null)
                    {
                        result = parameterEntry.SetValue(sample.Timestamp);
                    }
                }
            }
           
            return result;
        }

        private bool UpdateExternalTemperature()
        {
            bool result = false;
            
            if (GetExternalSample(out var sample))
            {
                if (sample.Source == SampleSource.Dht22 || sample.Source == SampleSource.Bmp180)
                {
                    if (_actualExternalTemperatureParameter != null)
                    {
                        result = _actualExternalTemperatureParameter.SetValue(sample.Temperature);
                    }

                    var parameterEntry = _device.SearchParameter("PARAM_ActualExternalTemperatureAveraged") as Parameter;

                    if (result && parameterEntry != null)
                    {
                        result = parameterEntry.SetValue(sample.TemperatureAveraged);
                    }

                    parameterEntry = _device.SearchParameter("PARAM_ExternalTemperatureTimestamp") as Parameter;

                    if (result && parameterEntry != null)
                    {
                        result = parameterEntry.SetValue(sample.Timestamp);
                    }
                }
            }

            return result;
        }

        private bool GetSds011Sample(out Pm pm)
        {
            bool result = false;
            float ipm25 = 0;
            float ipm10 = 0;

            pm = null;

            if (EltraSds011Wrapper.SdsPmRead("/dev/ttyUSB0", ref ipm25, ref ipm10) == 0)
            {
                pm = new Pm(ActualHumidity, ipm25, ipm10);

                pm.Normalize();

                result = true;
            }

            return result;
        }

        private bool GetBmp180Sample(out ThermoSensorsSample sample)
        {
            bool result = false;

            sample = null;

            if (GetExternalSample(out var se) && se.Source == SampleSource.Bmp180)
            {
                sample = se;
                result = true;
            }
            else if (GetInternalSample(out var si) && si.Source == SampleSource.Bmp180)
            {
                sample = si;
                result = true;
            }

            return result;
        }

        private bool UpdatePressure()
        {
            bool result = false;
            
            if (GetBmp180Sample(out var sample))
            {
                var parameterEntry = _device.SearchParameter("PARAM_Pressure") as Parameter;

                if (parameterEntry != null)
                {
                    result = parameterEntry.SetValue(sample.Pressure);
                }

                var qnhParameterEntry = _device.SearchParameter("PARAM_Qnh") as Parameter;

                if (qnhParameterEntry != null)
                {
                    var qnh = new Qnh(Settings.Bmp180Settings);

                    result = qnhParameterEntry.SetValue(qnh.GetValue(sample.Pressure));
                }
            }

            return result;
        }

        private bool UpdateParticulateMatterSensor()
        {
            bool result = false;

            if (GetSds011Sample(out var pm))
            {
                var parameterEntry = _device.SearchParameter("PARAM_ActualPm25") as Parameter;

                if (parameterEntry != null)
                {
                    result = parameterEntry.SetValue(pm.Pm25);
                }

                parameterEntry = _device.SearchParameter("PARAM_ActualPm10") as Parameter;

                if (parameterEntry != null)
                {
                    result = parameterEntry.SetValue(pm.Pm10);
                }
            }

            return result;
        }

        private void StartDht22Manager()
        {
            StopDht22Manager();

            MsgLogger.WriteLine($"Start Temp/Hum Manager");

            _dhtManager = new ThermoSensorsManager(_device)
            {
                Settings = Settings
            };

            _dhtManager.Start();
        }

        private void StopDht22Manager()
        {
            if (_dhtManager != null)
            {
                _dhtManager.Stop();
            }
        }

        public bool GetInternalSample(out ThermoSensorsSample sample)
        {
            bool result = false;

            sample = null;

            if (_dhtManager != null)
            {
                sample = _dhtManager.GetInternalSample();

                if(sample!=null)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - GetInternalSample", $"temperature={sample.Temperature}, humidity={sample.Humidity}, time={sample.Timestamp}");

                    result = true;
                }
            }

            return result;
        }

        public bool GetExternalSample(out ThermoSensorsSample sample)
        {
            bool result = false;

            sample = null;

            if (_dhtManager != null)
            {
                sample = _dhtManager.GetExternalSample();

                if (sample != null)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - GetInternalSample", $"external temperature={sample.Temperature}, humidity={sample.Humidity}, time={sample.Timestamp}");

                    result = true;
                }
            }

            return result;
        }
        
        public override bool Stop()
        {
            StopDht22Manager();

            return base.Stop();
        }

        #endregion
    }
}

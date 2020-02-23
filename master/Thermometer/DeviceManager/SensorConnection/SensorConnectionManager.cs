using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using EltraCommon.Threads;
using EltraConnector.SyncAgent;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ThermoMaster.Settings;
using Thermometer.DeviceManager.Device;

namespace ThermoMaster.DeviceManager.SensorConnection
{
    class SensorConnectionManager : EltraThread
    {
        #region Private fields

        private readonly ThermoDevice _device;
        private readonly SyncCloudAgent _cloudAgent;
        private ThermoSensorsManager _dhtManager;

        private Parameter _actualTemperatureParameter;
        private Parameter _actualExternalTemperatureParameter;
        private Parameter _actualHumidityParameter;
        private Parameter _samplingTimeParameter;

        private DeviceSettings _settings;

        #endregion

        #region Constructors

        public SensorConnectionManager(ThermoDevice device, SyncCloudAgent cloudAgent)
        {
            _device = device;
            _cloudAgent = cloudAgent;
        }

        #endregion

        #region Properties
        
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
            _samplingTimeParameter = _device?.SearchParameter("PARAM_SamplingTime") as Parameter;

            return _actualHumidityParameter != null && _actualTemperatureParameter != null && _samplingTimeParameter != null;
        }
        
        protected override Task Execute()
        {
            const int minWaitInterval = 100;
            
            try
            {
                InitParameters();

                StartThermoSensorsManager();
                
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

            StopThermoSensorsManager();

            SetStopped();

            return base.Execute();
        }

        private bool UpdateSensorData()
        {
            bool result = UpdateTemperature();

            result &= UpdateHumidity();

            result &= UpdateExternalTemperature();

            result &= UpdatePressure();

            return result;
        }

        private bool UpdateHumidity()
        {
            bool result = false;
                        
            if (GetInternalSample(out var sample))
            {
                if (sample.Source == SampleSource.dht11)
                {
                    if (_actualHumidityParameter != null)
                    {
                        result = _actualHumidityParameter.SetValue(sample.Humidity);
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
                if (sample.Source == SampleSource.dht11 || sample.Source == SampleSource.bme280)
                {
                    if (_actualTemperatureParameter != null)
                    {
                        result = _actualTemperatureParameter.SetValue(sample.Temperature);
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
                if (sample.Source == SampleSource.dht11 || sample.Source == SampleSource.bme280)
                {
                    if (_actualExternalTemperatureParameter != null)
                    {
                        result = _actualExternalTemperatureParameter.SetValue(sample.Temperature);
                    }
                }
            }

            return result;
        }

        private bool GetBme280Sample(out ThermoSensorsSample sample)
        {
            bool result = false;

            sample = null;

            if (GetExternalSample(out var se) && se.Source == SampleSource.bme280)
            {
                sample = se;
                result = true;
            }
            else if (GetInternalSample(out var si) && si.Source == SampleSource.bme280)
            {
                sample = si;
                result = true;
            }

            return result;
        }

        private bool UpdatePressure()
        {
            bool result = false;
            
            if (GetBme280Sample(out var sample))
            {
                var parameterEntry = _device.SearchParameter("PARAM_Pressure") as Parameter;

                if (parameterEntry != null)
                {
                    result = parameterEntry.SetValue(sample.Pressure);
                }
            }

            return result;
        }
        
        private void StartThermoSensorsManager()
        {
            StopThermoSensorsManager();

            MsgLogger.WriteLine($"Start Temp/Hum Manager");

            _dhtManager = new ThermoSensorsManager(_device)
            {
                Settings = Settings
            };

            _dhtManager.Start();
        }

        private void StopThermoSensorsManager()
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
            StopThermoSensorsManager();

            return base.Stop();
        }

        #endregion
    }
}

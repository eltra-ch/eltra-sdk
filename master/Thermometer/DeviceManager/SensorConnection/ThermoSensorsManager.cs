using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using EltraCommon.Threads;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ThermoMaster.DeviceManager.Device;
using ThermoMaster.Settings;
using ThermoMaster.DeviceManager.SensorConnection.DataSource;

namespace ThermoMaster.DeviceManager.SensorConnection
{
    class ThermoSensorsManager : EltraThread
    {
        #region Private fields

        private DeviceSettings _settings;
        private ThermoSensorsSample _internalSample;
        private ThermoSensorsSample _externalSample;
        private readonly ThermoDeviceBase _device;
        private Parameter _samplingTimeParameter;

        #endregion

        #region Constructor

        public ThermoSensorsManager(ThermoDeviceBase device)
        {
            _device = device;
            _internalSample = new ThermoSensorsSample();
            _externalSample = new ThermoSensorsSample();
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
        
        public ushort SamplingTime
        {
            get 
            {
                ushort result = 0;

                if (_samplingTimeParameter != null)
                {
                    if (_samplingTimeParameter.GetValue(out ushort samplingTime))
                    {
                        result = samplingTime;
                    }

                }

                return result;
            }           
        }

        #endregion

        #region Events

        private void OnSettingsChanged()
        {
            if (Settings != null)
            {
                Restart();
            }
        }

        #endregion

        #region Methods

        public ThermoSensorsSample GetInternalSample()
        {
            var result = new ThermoSensorsSample(_internalSample);

            return result;
        }

        public ThermoSensorsSample GetExternalSample()
        {
            var result = new ThermoSensorsSample(_externalSample);

            return result;
        }

        private bool InitParameters()
        {   
            _samplingTimeParameter = _device?.SearchParameter("PARAM_SamplingTime") as Parameter;

            return _samplingTimeParameter != null;
        }
        
        protected override Task Execute()
        {
            InitParameters();

            const int minWaitInterval = 100;

            var samplingTime = SamplingTime;

            MsgLogger.WriteLine($"start dht22 sampling, sampling time = {samplingTime}");

            while (ShouldRun())
            {
                ReadDht22Pins();
                ReadBmp180();

                var validateIntervalWatch = new Stopwatch();

                validateIntervalWatch.Start();

                while (validateIntervalWatch.Elapsed < TimeSpan.FromSeconds(samplingTime) && ShouldRun())
                {
                    if (SamplingTime != samplingTime)
                    {
                        samplingTime = SamplingTime;

                        MsgLogger.WriteLine($"dht22 sampling time changed, new sampling time = {samplingTime}");
                    }

                    Thread.Sleep(minWaitInterval);
                }

                GC.Collect(2, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();
            }

            return base.Execute();
        }

        private bool ReadBme280(out double temperature, out double pressure)
        {
            bool result = false;
            
            temperature = double.NaN;
            pressure = double.NaN;

            try
            {
                var dataSource = new Bme280DataSource("/tmp/BME280.txt");

                if (dataSource.ReadBme280(out var t, out var p, out var dateTime))
                {
                    temperature = t;
                    pressure = p;

                    result = true;

                    MsgLogger.WriteDebug($"{GetType().Name} - ReadBmp180", $"new sample: temperature={temperature}, pressure={pressure}");
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - ReadBmp180", $"ReadBmp180 failed!");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadBmp180", e);
            }

            return result;
        }

        private void ReadDht22Pins()
        {
            if (DhtRead(out var temperature, out var humidity))
            {
                _internalSample.Set(SampleSource.dht11, temperature, humidity);
            }
        }

        private void ReadBmp180()
        {
            if (ReadBme280(out var temperature, out var pressure))
            {
                _externalSample.Set(SampleSource.bme280, temperature, pressure);
            }
        }

        private bool DhtRead(out double temperature, out double humidity)
        {
            bool result = false;
            
            temperature = double.NaN;
            humidity = double.NaN;

            try
            {
                var dataSource = new Dht11DataSource("/tmp/DHT11.txt");

                if (dataSource.ReadDht11(out var t, out var h, out var dateTime))
                {
                    temperature = t;
                    humidity = h;
                    
                    result = true;

                    MsgLogger.WriteDebug($"{GetType().Name} - ReadDht22Pins", $"new sample: temperature={temperature}, humidity={humidity}, datetime={dateTime}");
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - ReadDht22Pins", $"ReadDht22 failed!");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadDht22Pins", e);
            }

            return result;
        }
        
        #endregion
    }
}

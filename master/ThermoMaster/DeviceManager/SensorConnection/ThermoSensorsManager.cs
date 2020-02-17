using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using EltraCommon.Threads;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ThermoMaster.DeviceManager.Device;
using ThermoMaster.DeviceManager.Wrapper;
using ThermoMaster.Settings;

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

        private bool ReadBmp180(out double temperature, out double pressure, out double altitude)
        {
            bool result = false;
            double t = double.NaN;
            double p = double.NaN;
            double a = double.NaN;

            temperature = double.NaN;
            pressure = double.NaN;
            altitude = double.NaN;

            try
            {
                int r = EltraBmp180Wrapper.BmpRead(ref t, ref p, ref a);

                if (r == 1)
                {
                    temperature = t;
                    pressure = p;
                    altitude = a;

                    result = true;

                    MsgLogger.WriteDebug($"{GetType().Name} - ReadBmp180", $"new sample: temperature={temperature}, pressure={pressure}, altitude={altitude}");
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - ReadBmp180", $"ReadBmp180 failed, result = {r}!");
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
            MsgLogger.WriteDebug($"{GetType().Name} - ReadDht22Pins", $"dht22 pins: internal={Settings.Dht22Settings.InternalPin}, " +
                                    $"external={Settings.Dht22Settings.ExternalPin}");

            if(Settings.Dht22Settings.InternalPin > 0)
            {
                if (DhtRead((ushort)Settings.Dht22Settings.InternalPin, out var temperature, out var humidity))
                {
                    _internalSample.Set(SampleSource.Dht22, temperature, humidity);
                }
            }

            if(Settings.Dht22Settings.ExternalPin > 0)
            {
                if (DhtRead((ushort)Settings.Dht22Settings.InternalPin, out var temperature, out var humidity))
                {
                    _externalSample.Set(SampleSource.Dht22, temperature, humidity);
                }
            }
        }

        private void ReadBmp180()
        {
            MsgLogger.WriteDebug($"{GetType().Name} - ReadBmp180", $"bmp180 settings: active={Settings.Bmp180Settings.Active}, " +
                                    $"usae={Settings.Bmp180Settings.Usage}");


            if(Settings.Bmp180Settings.Active)
            {
                if (ReadBmp180(out var temperature, out var pressure, out var altitude))
                {
                    if (Settings.Bmp180Settings.Usage == TempSensorUsage.Internal)
                    {
                        _internalSample.Set(SampleSource.Bmp180, temperature, pressure, altitude);
                    } else if (Settings.Bmp180Settings.Usage == TempSensorUsage.External)
                    {
                        _externalSample.Set(SampleSource.Bmp180, temperature, pressure, altitude);
                    }
                }
            }
        }

        private bool DhtRead(ushort pin, out double temperature, out double humidity)
        {
            const int delay = 2000;
            const int retryCount = 1;

            bool result = false;
            double t = double.NaN;
            double h = double.NaN;

            temperature = double.NaN;
            humidity = double.NaN;

            try
            {
                if (EltraDht22Wrapper.DhtRead(pin, delay, retryCount, ref t, ref h) == 1)
                {
                    temperature = t;
                    humidity = h;
                    
                    result = true;

                    MsgLogger.WriteDebug($"{GetType().Name} - ReadDht22Pins", $"new sample ({pin}): temperature={temperature}, humidity={humidity}");
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - ReadDht22Pins", $"ReadDht22 pin={pin} failed! delay={delay}, retry count={retryCount}");
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

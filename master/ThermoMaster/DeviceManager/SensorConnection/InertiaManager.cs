using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ThermoMaster.DeviceManager.Device;

namespace ThermoMaster.DeviceManager.SensorConnection
{
    class InertiaManager
    {
        #region Private fields

        private ThermoDeviceBase _device;
        private Task _temperatureInertiaTask;
        private Parameter _reactionInertiaParameter;
        private Parameter _relayState1Parameter;
        private Parameter _actualTemperatureParameter;
        private Parameter _temperatureMaxParameter;
        private Parameter _temperatureMinParameter;
        private Parameter _samplingTimeParameter;
        private bool _shouldRun;

        #endregion

        #region Constructors
        
        public InertiaManager(ThermoDeviceBase device)
        {
            _shouldRun = true;
            _device = device;
        }

        #endregion

        #region Properties

        ushort ReactionInertia
        {
            get
            {
                ushort result = ushort.MaxValue;

                if (_reactionInertiaParameter != null)
                {
                    _reactionInertiaParameter.GetValue(out result);
                }

                return result;
            }
        }

        public double ActualTemperature
        {
            get
            {
                double result = double.MinValue;

                if (_actualTemperatureParameter != null)
                {
                    if (_actualTemperatureParameter.GetValue(out double temperature))
                    {
                        result = temperature;
                    }
                }

                return result;
            }
        }

        double TemperatureMin
        {
            get
            {
                double result = 0;

                if (_temperatureMinParameter != null)
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

        public ushort SamplingTime
        {
            get
            {
                ushort result = 0;

                if (_samplingTimeParameter != null)
                {
                    if(_samplingTimeParameter.GetValue(out ushort samplingTime))
                    {
                        result = samplingTime;
                    }
                }

                return result;
            }
        }

        #endregion

        #region Methods

        public void Stop()
        {
            _shouldRun = false;
        }

        public bool Start()
        {
            bool result = false;

            if (InitParameters())
            {
                _shouldRun = true;
                result = true;
            }

            return result;
        }

        private bool InitParameters()
        {
            if (_relayState1Parameter == null || _reactionInertiaParameter == null || _actualTemperatureParameter == null)
            {
                _relayState1Parameter = _device.SearchParameter("PARAM_RelayState_1") as Parameter;
                _reactionInertiaParameter = _device?.SearchParameter("PARAM_ReactionInertia") as Parameter;
                _actualTemperatureParameter = _device.SearchParameter("PARAM_ActualTemperature") as Parameter;
                _samplingTimeParameter = _device?.SearchParameter("PARAM_SamplingTime") as Parameter;
                _temperatureMinParameter = _device?.SearchParameter("PARAM_TemperatureMinThreshold") as Parameter;
                _temperatureMaxParameter = _device?.SearchParameter("PARAM_TemperatureMaxThreshold") as Parameter;
            }

            return _relayState1Parameter != null && _reactionInertiaParameter != null && _actualTemperatureParameter != null;
        }

        public void StartMinCheck()
        {
            if (_temperatureInertiaTask == null || _temperatureInertiaTask.IsCompleted)
            {
                _temperatureInertiaTask = Task.Run( () => TemperatureInertiaMin());
            }
        }

        public void StartMaxCheck()
        {
            if (_temperatureInertiaTask == null || _temperatureInertiaTask.IsCompleted)
            {
                _temperatureInertiaTask = Task.Run( () => TemperatureInertiaMax());
            }
        }

        public void StartInRangeCheck()
        {
            if (_temperatureInertiaTask == null || _temperatureInertiaTask.IsCompleted)
            {
                _temperatureInertiaTask = Task.Run( () => TemperatureInertiaInRange());
            }
        }

        private bool ShouldRun()
        {
            return _shouldRun;
        }

        private bool IsInRangeTemperatureStable()
        {
            int minWaitInterval = 1000;
            bool result = true;

            MsgLogger.WriteLine($"Start in range temperature inertia ({ReactionInertia} s) Task...");

            var validateIntervalWatch = new Stopwatch();

            validateIntervalWatch.Start();

            while (validateIntervalWatch.Elapsed < TimeSpan.FromSeconds(ReactionInertia) && ShouldRun())
            {
                if (ActualTemperature < TemperatureMin || ActualTemperature > TemperatureMax)
                {
                    MsgLogger.WriteLine($"Temperature in range = {TemperatureMin}/{TemperatureMax} unstable, quit inertia probing task");
                    result = false;
                    break;
                }

                Thread.Sleep(minWaitInterval);
            }

            return result;
        }

        private void TemperatureInertiaInRange()
        {
            const ushort ChannelOn = 0;
            const ushort ChannelOff = 1;

            if (IsInRangeTemperatureStable())
            {
                RelayTransition.MakeTransition(_device, _relayState1Parameter, ChannelOn, ChannelOff);                
            }
        }

        private bool IsMinTemperatureStable()
        {
            var validateIntervalWatch = new Stopwatch();
            int minWaitInterval = 1000;
            bool result = true;

            validateIntervalWatch.Start();

            while (validateIntervalWatch.Elapsed < TimeSpan.FromSeconds(ReactionInertia) && ShouldRun())
            {
                if (ActualTemperature > TemperatureMin)
                {
                    MsgLogger.WriteLine($"Temperature Min={TemperatureMin} unstable, quit inertia probing task");
                    result = false;
                    break;
                }
                
                Thread.Sleep(minWaitInterval);
            }

            return result;
        }


        private bool PullTemperatureUp()
        {
            bool result = false;

            MsgLogger.WriteFlow( $"Pull up to temperature {TemperatureMax}, sampling time = {SamplingTime} [s] ...");

            while (ShouldRun())
            {
                if (ActualTemperature >= TemperatureMax)
                {
                    MsgLogger.WriteLine($"Temperature {TemperatureMax} reached, quit pull up");
                    result = true;
                    break;
                }

                Thread.Sleep(TimeSpan.FromSeconds(SamplingTime));
            }

            return result;
        }

        private void TemperatureInertiaMin()
        {
            const ushort ChannelOn = 0;
            const ushort ChannelOff = 1;

            MsgLogger.WriteLine($"Start Min Temperature Inertia ({ReactionInertia} s) Task...");

            if (IsMinTemperatureStable())
            {
                RelayTransition.MakeTransition(_device, _relayState1Parameter, ChannelOff, ChannelOn);

                PullTemperatureUp();
            }
        }

        private bool IsMaxTemperatureStable()
        {
            int minWaitInterval = 1000;
            bool result = true;

            MsgLogger.WriteLine($"Start Max Temperature Inertia ({ReactionInertia} s) Task...");

            var validateIntervalWatch = new Stopwatch();

            validateIntervalWatch.Start();

            while (validateIntervalWatch.Elapsed < TimeSpan.FromSeconds(ReactionInertia) && ShouldRun())
            {
                if (ActualTemperature < TemperatureMax)
                {
                    MsgLogger.WriteLine($"Temperature Max {TemperatureMax} unstable, quit max inertia probing task");
                    result = false;
                    break;
                }
                
                Thread.Sleep(minWaitInterval);
            }

            return result;
        }

        private void TemperatureInertiaMax()
        {
            const ushort ChannelOff = 1;
            const ushort ChannelOn = 0;

            if (IsMaxTemperatureStable())
            {
                RelayTransition.MakeTransition(_device, _relayState1Parameter, ChannelOn, ChannelOff);                
            }
        }
    }

    #endregion
}

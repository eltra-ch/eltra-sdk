using System;
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using ThermoMaster.DeviceManager.Device;
using ThermoMaster.DeviceManager.Events;
using ThermoMaster.Settings;

namespace ThermoMaster.DeviceManager
{
    class ThermoDeviceCommunication
    {
        #region Private fields

        private static readonly object SyncObject = new object();

        private readonly ThermoDeviceBase _device;
        private readonly MasterSettings _settings;
        private ThermoVcs _vcs;
        
        private int[] _relayPins;

        private ushort[] _lastState;

        private double _minTemperature;
        private double _maxTemperature;

        private double _minHumidity;
        private double _maxHumidity;

        private ushort _samplingTime;
        private ushort _reactionInertia;

        private ushort _controlWord;
        private ushort _statusWord;

        #endregion

        #region Constructors

        public ThermoDeviceCommunication(ThermoDeviceBase device, MasterSettings settings)
        {            
            _device = device;
            _settings = settings;

            _device.StatusChanged += OnDeviceStatusChanged;
        }
        
        #endregion

        #region Events

        public event EventHandler<DeviceCommunicationEventArgs> StatusChanged;
        
        #endregion

        #region Events handler

        protected virtual void OnStatusChanged(DeviceCommunicationEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        private void OnDeviceStatusChanged(object sender, EventArgs e)
        {
            if (_device.Status == EltraCloudContracts.Contracts.Devices.DeviceStatus.Registered)
            {
                var updateTask = Task.Run(async () =>
                {
                    if (Vcs != null)
                    {
                        await Vcs.UpdateParameterValue("PARAM_TemperatureMinThreshold");
                        await Vcs.UpdateParameterValue("PARAM_TemperatureMaxThreshold");
                        await Vcs.UpdateParameterValue("PARAM_HumidityMinThreshold");
                        await Vcs.UpdateParameterValue("PARAM_HumidityMaxThreshold");
                        
                        await Vcs.UpdateParameterValue("PARAM_ControlWord");
                        await Vcs.UpdateParameterValue("PARAM_StatusWord");

                        await Vcs.UpdateParameterValue("PARAM_SamplingTime");
                        await Vcs.UpdateParameterValue("PARAM_ReactionInertia");
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - OnDeviceStatusChanged", "Vcs not defined!");
                    }
                });

                updateTask.Wait();

                (_device?.SearchParameter("PARAM_TemperatureMinThreshold") as Parameter)?.GetValue(out _minTemperature);
                (_device?.SearchParameter("PARAM_TemperatureMaxThreshold") as Parameter)?.GetValue(out _maxTemperature);

                (_device?.SearchParameter("PARAM_HumidityMinThreshold") as Parameter)?.GetValue(out _minHumidity);
                (_device.SearchParameter("PARAM_HumidityMaxThreshold") as Parameter)?.GetValue(out _maxHumidity);

                (_device?.SearchParameter("PARAM_SamplingTime") as Parameter)?.GetValue(out _samplingTime);
                (_device?.SearchParameter("PARAM_ReactionInertia") as Parameter)?.GetValue(out _reactionInertia);

                (_device?.SearchParameter("PARAM_ControlWord") as Parameter)?.GetValue(out _controlWord);
                (_device?.SearchParameter("PARAM_StatusWord") as Parameter)?.GetValue(out _statusWord);
            }
        }

        #endregion

        #region Properties

        protected ThermoVcs Vcs
        {
            get
            {
                return _vcs ?? (_vcs = new ThermoVcs(_device, _settings.UpdateInterval, _settings.Timeout)); ;
            }
        }

        public int KeyHandle { get; private set; }
        public ushort NodeId { get; set; }        
        public uint LastErrorCode { get; set; }
        
        public int[] RelayPins 
        { 
            get => _relayPins; 
            set 
            { 
                _relayPins = value; 
                OnPinsChanged(); 
            }
        }
        
        #endregion

        #region Methods

        protected virtual void OnPinsChanged()
        {
            MsgLogger.WriteLine($"Relay pins set {string.Join(",", RelayPins)}");

            _lastState = new ushort[RelayPins.Length];
        }
        
        public bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            bool result = false;
            
            MsgLogger.WriteFlow( $"Get value, parameter 0x{objectIndex:X4}, 0x{objectSubindex:X4}");

            switch (objectIndex)
            {
                case ThermoDeviceParameters.Pressure_Index:
                    {
                        if (objectSubindex == ThermoDeviceParameters.Pressure_SubIndex)
                        {
                            if (_device.GetBme280Sample(out var sample))
                            {
                                data = BitConverter.GetBytes(sample.Pressure);
                                result = true;
                            }
                        }
                    }
                    break;                
                case ThermoDeviceParameters.Temperature_Index:
                {
                    switch (objectSubindex)
                    {
                        case ThermoDeviceParameters.TemperatureValue_SubIndex:
                            {
                                if (_device.GetInternalSample(out var sample))
                                {
                                    data = BitConverter.GetBytes(sample.Temperature);
                                    result = true;
                                }
                            }
                            break;                        
                    }
                } break;
                case ThermoDeviceParameters.Humidity_Index:
                {
                    switch (objectSubindex)
                    {
                        case ThermoDeviceParameters.HumidityValue_SubIndex:
                            {
                                if (_device.GetInternalSample(out var sample))
                                {
                                    data = BitConverter.GetBytes(sample.Humidity);
                                    result = true;
                                }
                            }
                            break;                        
                    }
                } break;
                case ThermoDeviceParameters.ExternalTemperature_Index:
                {
                    switch (objectSubindex)
                    {
                        case ThermoDeviceParameters.ExternalTemperatureValue_SubIndex:
                            {
                                if (_device.GetExternalSample(out var sample))
                                {
                                    data = BitConverter.GetBytes(sample.Temperature);
                                    result = true;
                                }
                            }
                            break;
                    }
                } break;
                

                case ThermoDeviceParameters.Setup_Index:
                {
                    switch (objectSubindex)
                    {
                        case ThermoDeviceParameters.SamplingTime_SubIndex:
                        {
                            data = BitConverter.GetBytes(_samplingTime);
                            result = true;
                        }
                        break;
                        
                    }
                } break;
            }

            return result;
        }

        public bool SetObject(ushort objectIndex, byte objectSubindex, byte[] data)
        {
            bool result = false;

            MsgLogger.WriteFlow( $"Set value, parameter 0x{objectIndex:X4}, 0x{objectSubindex:X4}");

            switch (objectIndex)
            {                
                case ThermoDeviceParameters.Setup_Index:
                {
                    switch (objectSubindex)
                    {   
                        case ThermoDeviceParameters.SamplingTime_SubIndex:
                        {
                            ushort samplingTime = BitConverter.ToUInt16(data, 0);

                            if(UpdateParameterDictionaryValue(objectIndex, objectSubindex, samplingTime))
                            {
                                _samplingTime = samplingTime;
                                result = true;
                            }
                        }
                        break;
                    }                    
                } break;
            }
            
            return result;
        }

        private bool UpdateParameterDictionaryValue<T>(ushort objectIndex, byte objectSubindex, T newValue)
        {
            bool result = false;
            var param = _device?.SearchParameter(objectIndex, objectSubindex) as Parameter;

            if (param != null)
            {
                result = param.SetValue(newValue);
            }

            return result;
        }
        
        #endregion
    }
}

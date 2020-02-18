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
        private ushort _samplingTime;

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
                        await Vcs.UpdateParameterValue("PARAM_SamplingTime");
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - OnDeviceStatusChanged", "Vcs not defined!");
                    }
                });

                updateTask.Wait();                

                (_device?.SearchParameter("PARAM_SamplingTime") as Parameter)?.GetValue(out _samplingTime);
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
        
        #endregion

        #region Methods
                
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

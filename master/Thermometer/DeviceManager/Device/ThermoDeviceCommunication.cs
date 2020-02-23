using System;
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using EltraMaster.Device;
using ThermoMaster.Settings;

namespace Thermometer.DeviceManager.Device
{
    class ThermoDeviceCommunication : MasterDeviceCommunication
    {
        #region Private fields

        private readonly MasterSettings _settings;

        private ushort _samplingTime;

        #endregion

        #region Constructors

        public ThermoDeviceCommunication(MasterDevice device, MasterSettings settings)
            : base(device, settings.UpdateInterval, settings.Timeout)
        {
            _settings = settings;
        }

        #endregion

        #region Events handler

        protected override void OnInitialized()
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

            (Device?.SearchParameter("PARAM_SamplingTime") as Parameter)?.GetValue(out _samplingTime);
        }

        #endregion

        #region Properties

        public int KeyHandle { get; private set; }
        public ushort NodeId { get; set; }

        #endregion

        #region Methods

        public override bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            bool result = false;

            MsgLogger.WriteFlow($"Get value, parameter 0x{objectIndex:X4}, 0x{objectSubindex:X4}");

            switch (objectIndex)
            {
                case ThermoDeviceParameters.Pressure_Index:
                    {
                        if (objectSubindex == ThermoDeviceParameters.Pressure_SubIndex)
                        {
                            if ((Device as ThermoDevice).GetBme280Sample(out var sample))
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
                                    if ((Device as ThermoDevice).GetInternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.Temperature);
                                        result = true;
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case ThermoDeviceParameters.Humidity_Index:
                    {
                        switch (objectSubindex)
                        {
                            case ThermoDeviceParameters.HumidityValue_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetInternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.Humidity);
                                        result = true;
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case ThermoDeviceParameters.ExternalTemperature_Index:
                    {
                        switch (objectSubindex)
                        {
                            case ThermoDeviceParameters.ExternalTemperatureValue_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetExternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.Temperature);
                                        result = true;
                                    }
                                }
                                break;
                        }
                    }
                    break;


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
                    }
                    break;
            }

            return result;
        }

        public override bool SetObject(ushort objectIndex, byte objectSubindex, byte[] data)
        {
            bool result = false;

            MsgLogger.WriteFlow($"Set value, parameter 0x{objectIndex:X4}, 0x{objectSubindex:X4}");

            switch (objectIndex)
            {
                case ThermoDeviceParameters.Setup_Index:
                    {
                        switch (objectSubindex)
                        {
                            case ThermoDeviceParameters.SamplingTime_SubIndex:
                                {
                                    ushort samplingTime = BitConverter.ToUInt16(data, 0);

                                    if (UpdateParameterDictionaryValue(objectIndex, objectSubindex, samplingTime))
                                    {
                                        _samplingTime = samplingTime;
                                        result = true;
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }

            return result;
        }

        #endregion
    }
}

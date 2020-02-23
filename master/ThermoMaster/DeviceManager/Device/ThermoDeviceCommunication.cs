using System;
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using ThermoMaster.DeviceManager.Wrapper;
using EltraMaster.Os.Linux;
using ThermoMaster.Settings;
using EltraMaster.Device;

namespace ThermoMaster.DeviceManager.Device
{
    class ThermoDeviceCommunication : MasterDeviceCommunication
    {
        #region Private fields

        private static readonly object SyncObject = new object();

        private readonly MasterSettings _settings;

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

        public ThermoDeviceCommunication(ThermoDevice device, MasterSettings settings)
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

            (Device?.SearchParameter("PARAM_TemperatureMinThreshold") as Parameter)?.GetValue(out _minTemperature);
            (Device?.SearchParameter("PARAM_TemperatureMaxThreshold") as Parameter)?.GetValue(out _maxTemperature);

            (Device?.SearchParameter("PARAM_HumidityMinThreshold") as Parameter)?.GetValue(out _minHumidity);
            (Device.SearchParameter("PARAM_HumidityMaxThreshold") as Parameter)?.GetValue(out _maxHumidity);

            (Device?.SearchParameter("PARAM_SamplingTime") as Parameter)?.GetValue(out _samplingTime);
            (Device?.SearchParameter("PARAM_ReactionInertia") as Parameter)?.GetValue(out _reactionInertia);

            (Device?.SearchParameter("PARAM_ControlWord") as Parameter)?.GetValue(out _controlWord);
            (Device?.SearchParameter("PARAM_StatusWord") as Parameter)?.GetValue(out _statusWord);
        }

        #endregion

        #region Properties

        public int KeyHandle { get; private set; }
        public ushort NodeId { get; set; }

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

        public override bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            bool result = false;

            MsgLogger.WriteFlow($"Get value, parameter 0x{objectIndex:X4}, 0x{objectSubindex:X4}");

            switch (objectIndex)
            {
                case ThermoDeviceParameters.Altitude_Index:
                    {
                        if (objectSubindex == ThermoDeviceParameters.Altitude_SubIndex)
                        {
                            if ((Device as ThermoDevice).GetBmp180Sample(out var sample))
                            {
                                data = BitConverter.GetBytes(sample.Temperature);
                                result = true;
                            }
                        }
                    }
                    break;
                case ThermoDeviceParameters.Pressure_Index:
                    {
                        if (objectSubindex == ThermoDeviceParameters.Pressure_SubIndex)
                        {
                            if ((Device as ThermoDevice).GetBmp180Sample(out var sample))
                            {
                                data = BitConverter.GetBytes(sample.Pressure);
                                result = true;
                            }
                        }
                    }
                    break;

                case ThermoDeviceParameters.RelayState_Index:
                    {
                        if (objectSubindex >= ThermoDeviceParameters.RelayState1_SubIndex &&
                            objectSubindex <= ThermoDeviceParameters.RelayState4_SubIndex)
                        {
                            result = GetChannelState((ushort)(objectSubindex - 1), out var channelState);

                            if (result)
                            {
                                data = BitConverter.GetBytes(channelState);
                            }
                            else
                            {
                                MsgLogger.WriteError($"{GetType().Name} - GetObject", $"Get channel index failed!, parameter 0x{objectIndex:X4}, 0x{objectSubindex:X4}");
                            }
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - GetObject", $"Get value, wrong subindex, parameter 0x{objectIndex:X4}, 0x{objectSubindex:X4}");
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
                            case ThermoDeviceParameters.TemperatureAveragedValue_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetInternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.TemperatureAveraged);
                                        result = true;
                                    }
                                }
                                break;
                            case ThermoDeviceParameters.TemperatureTimestamp_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetInternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.Timestamp.Ticks);
                                        result = true;
                                    }
                                }
                                break;
                            case ThermoDeviceParameters.TemperatureMin_SubIndex:
                                {
                                    data = BitConverter.GetBytes(_minTemperature);
                                    result = true;
                                }
                                break;
                            case ThermoDeviceParameters.TemperatureMax_SubIndex:
                                {
                                    data = BitConverter.GetBytes(_maxTemperature);
                                    result = true;
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
                            case ThermoDeviceParameters.HumidityAveragedValue_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetInternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.HumidityAveraged);
                                        result = true;
                                    }
                                }
                                break;
                            case ThermoDeviceParameters.HumidityTimestamp_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetInternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.Timestamp.Ticks);
                                        result = true;
                                    }
                                }
                                break;
                            case ThermoDeviceParameters.HumidityMin_SubIndex:
                                {
                                    data = BitConverter.GetBytes(_minHumidity);
                                    result = true;
                                }
                                break;
                            case ThermoDeviceParameters.HumidityMax_SubIndex:
                                {
                                    data = BitConverter.GetBytes(_maxHumidity);
                                    result = true;
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
                            case ThermoDeviceParameters.ExternalTemperatureAveragedValue_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetExternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.TemperatureAveraged);
                                        result = true;
                                    }
                                }
                                break;
                            case ThermoDeviceParameters.ExternalTemperatureTimestamp_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetExternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.Timestamp.Ticks);
                                        result = true;
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case ThermoDeviceParameters.ExternalHumidity_Index:
                    {
                        switch (objectSubindex)
                        {
                            case ThermoDeviceParameters.ExternalHumidityValue_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetExternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.Humidity);
                                        result = true;
                                    }
                                }
                                break;
                            case ThermoDeviceParameters.ExternalHumidityAveragedValue_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetExternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.HumidityAveraged);
                                        result = true;
                                    }
                                }
                                break;
                            case ThermoDeviceParameters.ExternalHumidityTimestamp_SubIndex:
                                {
                                    if ((Device as ThermoDevice).GetExternalSample(out var sample))
                                    {
                                        data = BitConverter.GetBytes(sample.Timestamp.Ticks);
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
                            case ThermoDeviceParameters.ReactionInertia_SubIndex:
                                {
                                    data = BitConverter.GetBytes(_reactionInertia);
                                    result = true;
                                }
                                break;
                            case ThermoDeviceParameters.SamplingTime_SubIndex:
                                {
                                    data = BitConverter.GetBytes(_samplingTime);
                                    result = true;
                                }
                                break;

                        }
                    }
                    break;
                case ThermoDeviceParameters.ControlWord_Index:
                    data = BitConverter.GetBytes(_controlWord);
                    result = true;
                    break;
                case ThermoDeviceParameters.StatusWord_Index:
                    data = BitConverter.GetBytes(_statusWord);
                    result = true;
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
                case ThermoDeviceParameters.RelayState_Index:
                    {
                        if (objectSubindex >= ThermoDeviceParameters.RelayState1_SubIndex && objectSubindex <= ThermoDeviceParameters.RelayState4_SubIndex)
                        {
                            ushort channelState = BitConverter.ToUInt16(data, 0);

                            result = SetChannelState((ushort)(objectSubindex - 1), channelState);

                            if (result)
                            {
                                result = UpdateParameterDictionaryValue(objectIndex, objectSubindex, channelState);
                            }
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - SetObject", $"Set object failed! parameter 0x{objectIndex:X4}, 0x{objectSubindex:X4}");
                        }
                    }
                    break;
                case ThermoDeviceParameters.Temperature_Index:
                    {
                        switch (objectSubindex)
                        {
                            case ThermoDeviceParameters.TemperatureMin_SubIndex:
                                double minTemperature = BitConverter.ToDouble(data, 0);
                                if (UpdateParameterDictionaryValue(objectIndex, objectSubindex, minTemperature))
                                {
                                    _minTemperature = minTemperature;
                                    result = true;
                                }

                                break;
                            case ThermoDeviceParameters.TemperatureMax_SubIndex:
                                double maxTemperature = BitConverter.ToDouble(data, 0);
                                if (UpdateParameterDictionaryValue(objectIndex, objectSubindex, maxTemperature))
                                {
                                    _maxTemperature = maxTemperature;
                                    result = true;
                                }
                                break;
                        }
                    }
                    break;
                case ThermoDeviceParameters.Humidity_Index:
                    {
                        switch (objectSubindex)
                        {
                            case ThermoDeviceParameters.HumidityMin_SubIndex:
                                double minHumidity = BitConverter.ToDouble(data, 0);
                                if (UpdateParameterDictionaryValue(objectIndex, objectSubindex, minHumidity))
                                {
                                    _minHumidity = minHumidity;
                                    result = true;
                                }
                                break;
                            case ThermoDeviceParameters.HumidityMax_SubIndex:
                                double maxHumidity = BitConverter.ToDouble(data, 0);
                                if (UpdateParameterDictionaryValue(objectIndex, objectSubindex, maxHumidity))
                                {
                                    _maxHumidity = maxHumidity;
                                    result = true;
                                }
                                break;
                        }
                    }
                    break;
                case ThermoDeviceParameters.Setup_Index:
                    {
                        switch (objectSubindex)
                        {
                            case ThermoDeviceParameters.ReactionInertia_SubIndex:
                                {
                                    ushort reactionInertia = BitConverter.ToUInt16(data, 0);

                                    if (UpdateParameterDictionaryValue(objectIndex, objectSubindex, reactionInertia))
                                    {
                                        _reactionInertia = reactionInertia;
                                        result = true;
                                    }
                                }
                                break;
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
                case ThermoDeviceParameters.ControlWord_Index:
                    ushort controlWord = BitConverter.ToUInt16(data, 0);
                    if (UpdateParameterDictionaryValue(objectIndex, objectSubindex, controlWord))
                    {
                        _controlWord = controlWord;
                        result = true;
                    }
                    break;
            }

            return result;
        }

        public bool GetChannelState(ushort index, out ushort state)
        {
            bool result = false;
            uint lastErrorCode = 0;
            int pinValue = 0;

            state = 0;

            lock (SyncObject)
            {
                MsgLogger.WriteFlow($"get channel - {index} state ...");

                try
                {
                    if (RelayPins != null && RelayPins.Length > index)
                    {
                        if (SystemHelper.IsLinux)
                        {
                            EltraRelayWrapper.RelayRead((ushort)RelayPins[index], ref pinValue);

                            state = (byte)pinValue;

                            MsgLogger.WriteFlow($"get channel - index={index}, pin={RelayPins[index]} state success, value = {state}");

                            result = true;
                        }
                        else
                        {
                            MsgLogger.WriteLine(LogMsgType.Warning, "GPIO library is not supported on windows, simulate success");
                            state = _lastState[index];
                            result = true;
                        }
                    }
                    else if (RelayPins != null)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - GetChannelState", $"pin index '{index}' too large, max index = {RelayPins.Length - 1} !");
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - GetChannelState", "Pins not defined!");
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - GetChannelState", e);
                }
            }

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetChannelState(ushort index, ushort state)
        {
            bool result = false;
            uint lastErrorCode = 0;
            int pinValue = 0;

            lock (SyncObject)
            {
                MsgLogger.WriteFlow($"set channel - {index} state = {state} ...");

                try
                {
                    if (RelayPins != null && RelayPins.Length > index)
                    {
                        if (SystemHelper.IsLinux)
                        {
                            MsgLogger.WriteFlow($"digital write - {RelayPins[index]} state = {state} ...");

                            EltraRelayWrapper.RelayWrite((ushort)RelayPins[index], state);

                            EltraRelayWrapper.RelayRead((ushort)RelayPins[index], ref pinValue);

                            if (pinValue == state)
                            {
                                MsgLogger.WriteFlow($"set channel - index={index}, pin={RelayPins[index]} state -> {state} success");

                                result = true;
                            }
                            else
                            {
                                MsgLogger.WriteError($"{GetType().Name} - SetChannelState", $"set channel - {index} state -> {state} failed!");
                            }
                        }
                        else
                        {
                            MsgLogger.WriteLine(LogMsgType.Warning, "GPIO library is not supported on windows, simulate success");
                            _lastState[index] = state;
                            result = true;
                        }
                    }
                    else if (RelayPins != null)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - SetChannelState", $"pin index '{index}' too large, max index = {RelayPins.Length - 1} !");
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - SetChannelState", $"Pins not defined!");
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - SetChannelState", e);
                }
            }

            LastErrorCode = lastErrorCode;

            return result;
        }

        #endregion
    }
}

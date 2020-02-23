namespace ThermoMaster.DeviceManager.Device
{
    public class ThermoDeviceParameters
    {
        public const ushort RelayState_Index = 0x3141;
        public const byte RelayState_MaxSubIndex = 0x00;
        public const byte RelayState1_SubIndex = 0x01;
        public const byte RelayState2_SubIndex = 0x02;
        public const byte RelayState3_SubIndex = 0x03;
        public const byte RelayState4_SubIndex = 0x04;

        public const ushort Temperature_Index = 0x2000;
        public const byte Temperature_MaxSubIndex = 0x00;
        public const byte TemperatureValue_SubIndex = 0x01;
        public const byte TemperatureAveragedValue_SubIndex = 0x02;
        public const byte TemperatureTimestamp_SubIndex = 0x03;
        public const byte TemperatureMin_SubIndex = 0x04;
        public const byte TemperatureMax_SubIndex = 0x05;

        public const ushort Humidity_Index = 0x2001;
        public const byte Humidity_MaxSubIndex = 0x00;
        public const byte HumidityValue_SubIndex = 0x01;
        public const byte HumidityAveragedValue_SubIndex = 0x02;
        public const byte HumidityTimestamp_SubIndex = 0x03;
        public const byte HumidityMin_SubIndex = 0x04;
        public const byte HumidityMax_SubIndex = 0x05;

        public const ushort ExternalTemperature_Index = 0x2002;
        public const byte ExternalTemperature_MaxSubIndex = 0x00;
        public const byte ExternalTemperatureValue_SubIndex = 0x01;
        public const byte ExternalTemperatureAveragedValue_SubIndex = 0x02;
        public const byte ExternalTemperatureTimestamp_SubIndex = 0x03;

        public const ushort ExternalHumidity_Index = 0x2003;
        public const byte ExternalHumidity_MaxSubIndex = 0x00;
        public const byte ExternalHumidityValue_SubIndex = 0x01;
        public const byte ExternalHumidityAveragedValue_SubIndex = 0x02;
        public const byte ExternalHumidityTimestamp_SubIndex = 0x03;

        public const ushort Pressure_Index = 0x2100;
        public const byte Pressure_SubIndex = 0x00;

        public const ushort Altitude_Index = 0x2101;
        public const byte Altitude_SubIndex = 0x00;

        public const ushort Setup_Index = 0x3001;
        public const byte Setup_MaxSubIndex = 0x00;
        public const byte ReactionInertia_SubIndex = 0x01;
        public const byte SamplingTime_SubIndex = 0x02;

        public const ushort ControlWord_Index = 0x6040;
        public const byte ControlWord_SubIndex = 0x00;

        public const ushort StatusWord_Index = 0x6041;
        public const byte StatusWord_SubIndex = 0x00;
    }
}

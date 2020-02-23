namespace Thermometer.DeviceManager.Device
{
    public class ThermoDeviceParameters
    {
        public const ushort Temperature_Index = 0x2000;
        public const byte Temperature_MaxSubIndex = 0x00;
        public const byte TemperatureValue_SubIndex = 0x01;

        public const ushort Humidity_Index = 0x2001;
        public const byte Humidity_MaxSubIndex = 0x00;
        public const byte HumidityValue_SubIndex = 0x01;

        public const ushort ExternalTemperature_Index = 0x2002;
        public const byte ExternalTemperature_MaxSubIndex = 0x00;
        public const byte ExternalTemperatureValue_SubIndex = 0x01;

        public const ushort Pressure_Index = 0x2100;
        public const byte Pressure_SubIndex = 0x00;

        public const ushort Setup_Index = 0x3001;
        public const byte Setup_MaxSubIndex = 0x00;
        public const byte SamplingTime_SubIndex = 0x01;
    }
}

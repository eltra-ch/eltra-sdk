namespace PhotoMaster.DeviceManager.Device
{
    public class PhotoDeviceParameters
    {
        public const ushort StatusWord_Index = 0x6041;
        public const byte StatusWord_SubIndex = 0x00;

        public const ushort InternalRecorderBufferVariable1 = 0x31E6;
        public const byte InternalRecorderBufferVariable1MaxSubIdx = 0x00;
        public const byte InternalRecorderBufferVariable1MaxSubIdxValue = 0x01;
        public const byte InternalRecorderBufferVariable1Data = 0x01;
    }
}

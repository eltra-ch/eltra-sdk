using System;
using EposMaster.DeviceManager.Device;
using EposMaster.DeviceManager.Identification.Events;

namespace EposMaster.DeviceManager.Identification
{
    class Epos4DeviceIdentification : EposDeviceIdentification
    {
        public Epos4DeviceIdentification(EposDevice device) : base(device)
        {
        }

        protected override bool ReadSerialNumber()
        {
            bool result = false;
            const ushort indexSerialNumber = 0x2100;
            const byte subIndexSerialNumber = 0x01;
            uint numberOfBytesRead = 8;
            byte[] data = new byte[numberOfBytesRead];
            var communication = Device?.Communication as Epos4DeviceCommunication;
            uint errorCode = 0;

            try
            {
                if (communication != null && communication.Connected)
                {
                    if (VcsWrapper.Device.VcsGetObject(
                            communication.KeyHandle,
                            communication.NodeId,
                            indexSerialNumber,
                            subIndexSerialNumber,
                            data,
                            numberOfBytesRead,
                            ref numberOfBytesRead,
                            ref errorCode) > 0)
                    {
                        SerialNumber = BitConverter.ToUInt64(data,0);

                        if (SerialNumber == 0)
                        {
                            SerialNumber = UInt64.MaxValue;
                        }

                        result = SerialNumber > 0;
                    }
                }
            }
            catch (Exception e)
            {
                _lastException = e;
            }

            return result;
        }
    }
}
